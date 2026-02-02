import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialog } from '@angular/material/dialog';
import { ProgramsApi, CourseDto, ProgramDto } from '../../../core/api/programs.api';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { SkeletonCardsComponent } from '../../../shared/skeleton/skeleton-cards.component';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';
import { CourseDialogComponent } from './course-dialog.component';

@Component({
  selector: 'app-courses',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    NgFor,
    NgIf,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule,
    PaginationComponent,
    SkeletonCardsComponent,
  ],
  templateUrl: './courses.component.html',
  styleUrl: './courses.component.scss',
})
export class CoursesComponent implements OnInit {
  private readonly api = inject(ProgramsApi);
  private readonly dialog = inject(MatDialog);
  private readonly confirm = inject(ConfirmDialogService);

  programs: ProgramDto[] = [];
  courses: CourseDto[] = [];
  selectedProgramId: string | null = null;
  page = 1;
  pageSize = 10;
  total = 0;
  isLoading = false;

  ngOnInit(): void {
    this.loadPrograms();
    this.loadCourses();
  }

  loadPrograms(): void {
    this.api.listPrograms(1, 100).subscribe((response) => {
      this.programs = response.items;
    });
  }

  loadCourses(): void {
    this.isLoading = true;
    this.api.listCourses(this.selectedProgramId, this.page, this.pageSize).subscribe({
      next: (response) => {
        this.courses = response.items;
        this.page = response.page;
        this.pageSize = response.pageSize;
        this.total = response.total;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  onProgramChange(): void {
    this.page = 1;
    this.loadCourses();
  }

  openCreate(): void {
    const ref = this.dialog.open(CourseDialogComponent, {
      width: '460px',
      data: { programs: this.programs },
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.createCourse(result).subscribe(() => this.loadCourses());
    });
  }

  openEdit(course: CourseDto): void {
    const ref = this.dialog.open(CourseDialogComponent, {
      width: '460px',
      data: { course, programs: this.programs },
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.updateCourse(course.id, result).subscribe(() => this.loadCourses());
    });
  }

  delete(course: CourseDto): void {
    this.confirm
      .confirm({
        title: 'Delete course',
        message: `Delete ${course.name}?`,
        confirmText: 'Delete',
      })
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }
        this.api.deleteCourse(course.id).subscribe(() => this.loadCourses());
      });
  }

  onPageChange(event: { page: number; pageSize: number }): void {
    this.page = event.page;
    this.pageSize = event.pageSize;
    this.loadCourses();
  }
}
