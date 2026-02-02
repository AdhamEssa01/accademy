import { Injectable } from '@angular/core';
import { ApiClient } from './api-client';

@Injectable({ providedIn: 'root' })
export class DashboardsApi {
  constructor(private readonly api: ApiClient) {}

  getAdmin() {
    return this.api.get<Record<string, unknown>>('/v1/dashboards/admin');
  }

  getInstructor() {
    return this.api.get<Record<string, unknown>>('/v1/dashboards/instructor');
  }

  getParent() {
    return this.api.get<Record<string, unknown>>('/v1/dashboards/parent');
  }
}
