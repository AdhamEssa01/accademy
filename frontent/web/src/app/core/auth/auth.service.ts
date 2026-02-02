import { Injectable, NgZone } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, Subscription, distinctUntilChanged, finalize, interval, of, shareReplay, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthResponse,
  LoginRequest,
  LogoutRequest,
  RefreshRequest,
  RegisterRequest,
  UserInfo,
} from './auth.models';
import { Roles } from './roles';

const ACCESS_TOKEN_KEY = 'academy.accessToken';
const REFRESH_TOKEN_KEY = 'academy.refreshToken';
const EXPIRES_AT_KEY = 'academy.expiresAt';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiBase = environment.apiBaseUrl;
  private readonly userSubject = new BehaviorSubject<UserInfo | null>(null);
  private readonly expiresAtSubject = new BehaviorSubject<number | null>(this.readExpiresAt());
  private refreshInFlight?: Observable<AuthResponse>;

  readonly user$ = this.userSubject.asObservable();
  readonly sessionExpiring$ = this.createSessionExpiringStream();

  constructor(
    private readonly http: HttpClient,
    private readonly zone: NgZone
  ) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiBase}/v1/auth/login`, request)
      .pipe(tap((response) => this.setSession(response)));
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.apiBase}/v1/auth/register`, request)
      .pipe(tap((response) => this.setSession(response)));
  }

  refresh(): Observable<AuthResponse> {
    if (this.refreshInFlight) {
      return this.refreshInFlight;
    }

    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('Missing refresh token.'));
    }

    this.refreshInFlight = this.http
      .post<AuthResponse>(`${this.apiBase}/v1/auth/refresh`, {
        refreshToken,
      } satisfies RefreshRequest)
      .pipe(
        tap((response) => this.setSession(response)),
        finalize(() => {
          this.refreshInFlight = undefined;
        }),
        shareReplay({ bufferSize: 1, refCount: true })
      );

    return this.refreshInFlight;
  }

  logout(): Observable<void> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      this.clearSession();
      return of(void 0);
    }

    return this.http
      .post<void>(`${this.apiBase}/v1/auth/logout`, {
        refreshToken,
      } satisfies LogoutRequest)
      .pipe(
        finalize(() => {
          this.clearSession();
        })
      );
  }

  me(): Observable<UserInfo> {
    return this.http.get<UserInfo>(`${this.apiBase}/v1/auth/me`).pipe(
      tap((user) => this.setUser(user))
    );
  }

  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  getUser(): UserInfo | null {
    return this.userSubject.value;
  }

  isAuthenticated(): boolean {
    return !!this.getAccessToken();
  }

  hasAnyRole(roles: string[]): boolean {
    const userRoles = this.getUser()?.roles ?? [];
    return roles.some((role) => userRoles.includes(role));
  }

  getDefaultRoute(): string {
    const roles = this.getUser()?.roles ?? [];

    if (roles.includes(Roles.Admin)) {
      return '/app/admin/academy';
    }
    if (roles.includes(Roles.Instructor)) {
      return '/app/instructor/groups';
    }
    if (roles.includes(Roles.Parent)) {
      return '/app/parent/children';
    }

    return '/app';
  }

  clearSession(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(EXPIRES_AT_KEY);
    this.userSubject.next(null);
    this.expiresAtSubject.next(null);
  }

  private setSession(response: AuthResponse): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    const expiresAt = Date.now() + response.expiresInSeconds * 1000;
    localStorage.setItem(EXPIRES_AT_KEY, expiresAt.toString());
    this.expiresAtSubject.next(expiresAt);
    this.setUser(response.userInfo);
  }

  private setUser(user: UserInfo): void {
    this.userSubject.next(user);
  }

  getSessionRemainingSeconds(): number | null {
    const expiresAt = this.expiresAtSubject.value ?? this.readExpiresAt();
    if (!expiresAt) {
      return null;
    }
    return Math.max(0, Math.floor((expiresAt - Date.now()) / 1000));
  }

  private readExpiresAt(): number | null {
    const raw = localStorage.getItem(EXPIRES_AT_KEY);
    if (!raw) {
      return null;
    }
    const value = Number(raw);
    return Number.isNaN(value) ? null : value;
  }

  private createSessionExpiringStream(): Observable<boolean> {
    return new Observable<boolean>((subscriber) => {
      const emit = () => {
        const remaining = this.getSessionRemainingSeconds();
        const isExpiring = remaining !== null && remaining <= 120;
        subscriber.next(isExpiring);
      };

      let intervalSub: Subscription | undefined;

      this.zone.runOutsideAngular(() => {
        this.zone.run(emit);
        intervalSub = interval(10000).subscribe(() => {
          this.zone.run(emit);
        });
      });

      return () => {
        intervalSub?.unsubscribe();
      };
    }).pipe(
      distinctUntilChanged(),
      shareReplay({ bufferSize: 1, refCount: true })
    );
  }
}
