import { Injectable } from '@angular/core';
import { ApiClient } from './api-client';
import { PagedResponse } from '../../shared/pagination/paging.models';

export type AttendanceStatus = 'Present' | 'Absent' | 'Late' | 'Excused';

export interface AttendanceRecordDto {
  id: string;
  sessionId: string;
  studentId: string;
  status: AttendanceStatus;
  reason?: string | null;
  note?: string | null;
  markedAtUtc?: string;
}

export interface AttendanceItemRequest {
  studentId: string;
  status: AttendanceStatus;
  reason?: string | null;
  note?: string | null;
}

export interface SubmitAttendanceRequest {
  items: AttendanceItemRequest[];
}

@Injectable({ providedIn: 'root' })
export class AttendanceApi {
  constructor(private readonly api: ApiClient) {}

  getSessionAttendance(sessionId: string) {
    return this.api.get<AttendanceRecordDto[]>(`/v1/sessions/${sessionId}/attendance`);
  }

  submitSessionAttendance(sessionId: string, request: SubmitAttendanceRequest) {
    return this.api.post<void>(`/v1/sessions/${sessionId}/attendance`, request);
  }

  listAttendance(params: {
    groupId?: string | null;
    studentId?: string | null;
    from?: string | null;
    to?: string | null;
    status?: AttendanceStatus | null;
    page: number;
    pageSize: number;
  }) {
    return this.api.get<PagedResponse<AttendanceRecordDto>>('/v1/attendance', params);
  }

  listMyChildrenAttendance(page: number, pageSize: number, from?: string, to?: string) {
    return this.api.get<PagedResponse<AttendanceRecordDto>>('/v1/parent/me/attendance', {
      page,
      pageSize,
      from,
      to,
    });
  }
}
