import { Component, inject } from '@angular/core';
import { AsyncPipe, NgIf } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-session-banner',
  standalone: true,
  imports: [NgIf, AsyncPipe, MatButtonModule],
  templateUrl: './session-banner.component.html',
  styleUrl: './session-banner.component.scss',
})
export class SessionBannerComponent {
  private readonly auth = inject(AuthService);
  readonly sessionExpiring$ = this.auth.sessionExpiring$;

  refreshSession(): void {
    this.auth.refresh().subscribe();
  }
}
