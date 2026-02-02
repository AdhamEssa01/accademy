import { Injectable } from '@angular/core';
import { ApiClient } from './api-client';
import { PagedResponse } from '../../shared/pagination/paging.models';

export interface ProgramDto {
  id: string;
  name: string;
  description?: string | null;
}

export interface CourseDto {
  id: string;
  programId: string;
  name: string;
  description?: string | null;
}

export interface LevelDto {
  id: string;
  courseId: string;
  name: string;
  sortOrder: number;
}

export interface CreateProgramRequest {
  name: string;
  description?: string | null;
}

export interface UpdateProgramRequest {
  name: string;
  description?: string | null;
}

export interface CreateCourseRequest {
  programId: string;
  name: string;
  description?: string | null;
}

export interface UpdateCourseRequest {
  name: string;
  description?: string | null;
}

export interface CreateLevelRequest {
  courseId: string;
  name: string;
  sortOrder: number;
}

export interface UpdateLevelRequest {
  name: string;
  sortOrder: number;
}

@Injectable({ providedIn: 'root' })
export class ProgramsApi {
  constructor(private readonly api: ApiClient) {}

  listPrograms(page: number, pageSize: number) {
    return this.api.get<PagedResponse<ProgramDto>>('/v1/programs', { page, pageSize });
  }

  createProgram(request: CreateProgramRequest) {
    return this.api.post<ProgramDto>('/v1/programs', request);
  }

  updateProgram(id: string, request: UpdateProgramRequest) {
    return this.api.put<ProgramDto>(`/v1/programs/${id}`, request);
  }

  deleteProgram(id: string) {
    return this.api.delete<void>(`/v1/programs/${id}`);
  }

  listCourses(programId: string | null, page: number, pageSize: number) {
    return this.api.get<PagedResponse<CourseDto>>('/v1/courses', {
      programId: programId || undefined,
      page,
      pageSize,
    });
  }

  createCourse(request: CreateCourseRequest) {
    return this.api.post<CourseDto>('/v1/courses', request);
  }

  updateCourse(id: string, request: UpdateCourseRequest) {
    return this.api.put<CourseDto>(`/v1/courses/${id}`, request);
  }

  deleteCourse(id: string) {
    return this.api.delete<void>(`/v1/courses/${id}`);
  }

  listLevels(courseId: string | null, page: number, pageSize: number) {
    return this.api.get<PagedResponse<LevelDto>>('/v1/levels', {
      courseId: courseId || undefined,
      page,
      pageSize,
    });
  }

  createLevel(request: CreateLevelRequest) {
    return this.api.post<LevelDto>('/v1/levels', request);
  }

  updateLevel(id: string, request: UpdateLevelRequest) {
    return this.api.put<LevelDto>(`/v1/levels/${id}`, request);
  }

  deleteLevel(id: string) {
    return this.api.delete<void>(`/v1/levels/${id}`);
  }
}
