import { Injectable } from '@angular/core';
import { ApiClient } from './api-client';

export interface AcademyDto {
  id: string;
  name: string;
  createdAtUtc?: string;
}

export interface UpdateAcademyRequest {
  name: string;
}

@Injectable({ providedIn: 'root' })
export class AcademyApi {
  constructor(private readonly api: ApiClient) {}

  getMine() {
    return this.api.get<AcademyDto>('/v1/academies/me');
  }

  updateMine(request: UpdateAcademyRequest) {
    return this.api.put<AcademyDto>('/v1/academies/me', request);
  }
}
