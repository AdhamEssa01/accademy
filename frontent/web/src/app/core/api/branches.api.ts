import { Injectable } from '@angular/core';
import { ApiClient } from './api-client';
import { PagedResponse } from '../../shared/pagination/paging.models';

export interface BranchDto {
  id: string;
  academyId?: string;
  name: string;
  address?: string | null;
}

export interface CreateBranchRequest {
  name: string;
  address?: string | null;
}

export interface UpdateBranchRequest {
  name: string;
  address?: string | null;
}

@Injectable({ providedIn: 'root' })
export class BranchesApi {
  constructor(private readonly api: ApiClient) {}

  list(page: number, pageSize: number) {
    return this.api.get<PagedResponse<BranchDto>>('/v1/branches', { page, pageSize });
  }

  get(id: string) {
    return this.api.get<BranchDto>(`/v1/branches/${id}`);
  }

  create(request: CreateBranchRequest) {
    return this.api.post<BranchDto>('/v1/branches', request);
  }

  update(id: string, request: UpdateBranchRequest) {
    return this.api.put<BranchDto>(`/v1/branches/${id}`, request);
  }

  delete(id: string) {
    return this.api.delete<void>(`/v1/branches/${id}`);
  }
}
