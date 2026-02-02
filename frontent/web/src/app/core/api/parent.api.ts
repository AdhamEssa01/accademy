import { Injectable } from '@angular/core';
import { ApiClient } from './api-client';

export interface ParentChildDto {
  id: string;
  fullName: string;
  photoUrl?: string | null;
}

export interface AssignmentDto {
  id: string;
  title: string;
  dueDate?: string | null;
  description?: string | null;
}

export interface AnnouncementDto {
  id: string;
  title: string;
  body?: string | null;
  publishedAtUtc?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ParentApi {
  constructor(private readonly api: ApiClient) {}

  getChildren() {
    return this.api.get<ParentChildDto[]>('/v1/parent/me/children');
  }

  getAssignments(page: number, pageSize: number) {
    return this.api.get<{ items: AssignmentDto[]; page: number; pageSize: number; total: number }>(
      '/v1/parent/me/assignments',
      { page, pageSize }
    );
  }

  getAnnouncements(page: number, pageSize: number) {
    return this.api.get<{ items: AnnouncementDto[]; page: number; pageSize: number; total: number }>(
      '/v1/parent/me/announcements',
      { page, pageSize }
    );
  }
}
