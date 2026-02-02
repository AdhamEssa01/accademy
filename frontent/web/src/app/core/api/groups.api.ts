import { Injectable } from '@angular/core';
import { ApiClient } from './api-client';
import { PagedResponse } from '../../shared/pagination/paging.models';

export interface GroupDto {
  id: string;
  name: string;
  programId: string;
  courseId: string;
  levelId: string;
  instructorUserId?: string | null;
}

export interface SessionDto {
  id: string;
  groupId: string;
  instructorUserId: string;
  startsAtUtc: string;
  durationMinutes: number;
  notes?: string | null;
}

export interface CreateGroupRequest {
  name: string;
  programId: string;
  courseId: string;
  levelId: string;
  instructorUserId?: string | null;
}

export interface UpdateGroupRequest {
  name: string;
  instructorUserId?: string | null;
}

export interface CreateSessionRequest {
  groupId: string;
  instructorUserId: string;
  startsAtUtc: string;
  durationMinutes: number;
  notes?: string | null;
}

export interface UpdateSessionRequest {
  startsAtUtc: string;
  durationMinutes: number;
  notes?: string | null;
}

@Injectable({ providedIn: 'root' })
export class GroupsApi {
  constructor(private readonly api: ApiClient) {}

  listGroups(page: number, pageSize: number) {
    return this.api.get<PagedResponse<GroupDto>>('/v1/groups', { page, pageSize });
  }

  listMyGroups(page: number, pageSize: number) {
    return this.api.get<PagedResponse<GroupDto>>('/v1/groups/mine', { page, pageSize });
  }

  createGroup(request: CreateGroupRequest) {
    return this.api.post<GroupDto>('/v1/groups', request);
  }

  updateGroup(id: string, request: UpdateGroupRequest) {
    return this.api.put<GroupDto>(`/v1/groups/${id}`, request);
  }

  deleteGroup(id: string) {
    return this.api.delete<void>(`/v1/groups/${id}`);
  }

  listSessions(page: number, pageSize: number, from?: string, to?: string) {
    return this.api.get<PagedResponse<SessionDto>>('/v1/sessions', {
      page,
      pageSize,
      from,
      to,
    });
  }

  listMySessions(page: number, pageSize: number, from?: string, to?: string) {
    return this.api.get<PagedResponse<SessionDto>>('/v1/sessions/mine', {
      page,
      pageSize,
      from,
      to,
    });
  }

  createSession(request: CreateSessionRequest) {
    return this.api.post<SessionDto>('/v1/sessions', request);
  }

  updateSession(id: string, request: UpdateSessionRequest) {
    return this.api.put<SessionDto>(`/v1/sessions/${id}`, request);
  }

  deleteSession(id: string) {
    return this.api.delete<void>(`/v1/sessions/${id}`);
  }
}
