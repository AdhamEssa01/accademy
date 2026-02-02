import { Injectable } from '@angular/core';
import { ApiClient } from './api-client';
import { Observable, of, tap, catchError } from 'rxjs';
import { PagedResponse } from '../../shared/pagination/paging.models';

export interface AchievementDto {
  id: string;
  title: string;
  description?: string | null;
  tag?: string | null;
  imageUrl?: string | null;
}

export interface CreateAchievementRequest {
  title: string;
  description?: string | null;
  tag?: string | null;
  imageUrl?: string | null;
}

export interface UpdateAchievementRequest {
  title: string;
  description?: string | null;
  tag?: string | null;
  imageUrl?: string | null;
}

interface CacheEntry<T> {
  expiresAt: number;
  value: T;
}

@Injectable({ providedIn: 'root' })
export class AchievementsApi {
  private readonly cache = new Map<string, CacheEntry<PagedResponse<AchievementDto>>>();
  private readonly ttlMs = 5 * 60 * 1000;

  constructor(private readonly api: ApiClient) {}

  listPublic(page: number, pageSize: number, tag?: string | null): Observable<PagedResponse<AchievementDto>> {
    const cacheKey = `achievements:${page}:${pageSize}:${tag ?? ''}`;
    const cached = this.cache.get(cacheKey);
    if (cached && cached.expiresAt > Date.now()) {
      return of(cached.value);
    }

    return this.api
      .get<PagedResponse<AchievementDto>>('/v1/public/achievements', { page, pageSize, tag: tag || undefined })
      .pipe(
        tap((response) => this.cache.set(cacheKey, { value: response, expiresAt: Date.now() + this.ttlMs })),
        catchError(() =>
          of({ items: [], page, pageSize, total: 0 } as PagedResponse<AchievementDto>)
        )
      );
  }

  listAdmin(page: number, pageSize: number) {
    return this.api.get<PagedResponse<AchievementDto>>('/v1/achievements', { page, pageSize });
  }

  create(request: CreateAchievementRequest) {
    return this.api.post<AchievementDto>('/v1/achievements', request);
  }

  update(id: string, request: UpdateAchievementRequest) {
    return this.api.put<AchievementDto>(`/v1/achievements/${id}`, request);
  }

  delete(id: string) {
    return this.api.delete<void>(`/v1/achievements/${id}`);
  }
}
