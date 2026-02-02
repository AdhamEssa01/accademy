import { Injectable } from '@angular/core';
import { ApiClient } from './api-client';
import { PagedResponse } from '../../shared/pagination/paging.models';

export interface StudentDto {
  id: string;
  fullName: string;
  dateOfBirth?: string | null;
  photoUrl?: string | null;
  notes?: string | null;
}

export interface CreateStudentRequest {
  fullName: string;
  dateOfBirth?: string | null;
  notes?: string | null;
}

export interface UpdateStudentRequest {
  fullName: string;
  dateOfBirth?: string | null;
  notes?: string | null;
}

@Injectable({ providedIn: 'root' })
export class StudentsApi {
  constructor(private readonly api: ApiClient) {}

  list(page: number, pageSize: number) {
    return this.api.get<PagedResponse<StudentDto>>('/v1/students', { page, pageSize });
  }

  get(id: string) {
    return this.api.get<StudentDto>(`/v1/students/${id}`);
  }

  create(request: CreateStudentRequest) {
    return this.api.post<StudentDto>('/v1/students', request);
  }

  update(id: string, request: UpdateStudentRequest) {
    return this.api.put<StudentDto>(`/v1/students/${id}`, request);
  }

  delete(id: string) {
    return this.api.delete<void>(`/v1/students/${id}`);
  }

  uploadPhoto(id: string, file: File) {
    const form = new FormData();
    form.append('file', file);
    return this.api.postForm<StudentDto>(`/v1/students/${id}/photo`, form);
  }
}
