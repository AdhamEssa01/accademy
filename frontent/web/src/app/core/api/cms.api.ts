import { Injectable } from '@angular/core';
import { ApiClient } from './api-client';
import { Observable, of, tap, catchError } from 'rxjs';

export interface CmsSection {
  type: string;
  jsonContent?: string | null;
  content?: unknown;
}

export interface CmsPage {
  id?: string;
  slug?: string;
  title?: string;
  sections?: CmsSection[];
}

interface CacheEntry<T> {
  expiresAt: number;
  value: T | null;
}

@Injectable({ providedIn: 'root' })
export class CmsApi {
  private readonly cache = new Map<string, CacheEntry<CmsPage>>();
  private readonly ttlMs = 5 * 60 * 1000;

  constructor(private readonly api: ApiClient) {}

  getPage(slug: string): Observable<CmsPage | null> {
    const cacheKey = `cms:${slug}`;
    const cached = this.cache.get(cacheKey);
    if (cached && cached.expiresAt > Date.now()) {
      return of(cached.value);
    }

    return this.api.get<CmsPage>(`/v1/public/cms/pages/${slug}`).pipe(
      tap((page) => this.cache.set(cacheKey, { value: page, expiresAt: Date.now() + this.ttlMs })),
      catchError(() => {
        this.cache.set(cacheKey, { value: null, expiresAt: Date.now() + 60_000 });
        return of(null);
      })
    );
  }
}
