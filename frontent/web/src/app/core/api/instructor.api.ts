import { Injectable } from '@angular/core';
import { ApiClient } from './api-client';
import { PagedResponse } from '../../shared/pagination/paging.models';
import { GroupDto, SessionDto } from './groups.api';
import { AssignmentDto } from './parent.api';

@Injectable({ providedIn: 'root' })
export class InstructorApi {
  constructor(private readonly api: ApiClient) {}

  listMyGroups(page: number, pageSize: number) {
    return this.api.get<PagedResponse<GroupDto>>('/v1/groups/mine', { page, pageSize });
  }

  listMySessions(page: number, pageSize: number, from?: string, to?: string) {
    return this.api.get<PagedResponse<SessionDto>>('/v1/sessions/mine', {
      page,
      pageSize,
      from,
      to,
    });
  }

  listAssignments(page: number, pageSize: number) {
    return this.api.get<PagedResponse<AssignmentDto>>('/v1/instructor/assignments', { page, pageSize });
  }
}
