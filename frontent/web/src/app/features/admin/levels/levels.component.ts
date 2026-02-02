import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialog } from '@angular/material/dialog';
import { ProgramsApi, CourseDto, LevelDto } from '../../../core/api/programs.api';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { SkeletonCardsComponent } from '../../../shared/skeleton/skeleton-cards.component';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';
import { LevelDialogComponent } from './level-dialog.component';

@Component({
  selector: 'app-levels',
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
  templateUrl: './levels.component.html',
  styleUrl: './levels.component.scss',
})
export class LevelsComponent implements OnInit {
  private readonly api = inject(ProgramsApi);
  private readonly dialog = inject(MatDialog);
  private readonly confirm = inject(ConfirmDialogService);

  courses: CourseDto[] = [];
  levels: LevelDto[] = [];
  selectedCourseId: string | null = null;
  page = 1;
  pageSize = 10;
  total = 0;
  isLoading = false;

  ngOnInit(): void {
    this.loadCourses();
    this.loadLevels();
  }

  loadCourses(): void {
    this.api.listCourses(null, 1, 100).subscribe((response) => {
      this.courses = response.items;
    });
  }

  loadLevels(): void {
    this.isLoading = true;
    this.api.listLevels(this.selectedCourseId, this.page, this.pageSize).subscribe({
      next: (response) => {
        this.levels = response.items;
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

  onCourseChange(): void {
    this.page = 1;
    this.loadLevels();
  }

  openCreate(): void {
    const ref = this.dialog.open(LevelDialogComponent, {
      width: '460px',
      data: { courses: this.courses },
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.createLevel(result).subscribe(() => this.loadLevels());
    });
  }

  openEdit(level: LevelDto): void {
    const ref = this.dialog.open(LevelDialogComponent, {
      width: '460px',
      data: { level, courses: this.courses },
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.updateLevel(level.id, result).subscribe(() => this.loadLevels());
    });
  }

  delete(level: LevelDto): void {
    this.confirm
      .confirm({
        title: 'Delete level',
        message: `Delete ${level.name}?`,
        confirmText: 'Delete',
      })
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }
        this.api.deleteLevel(level.id).subscribe(() => this.loadLevels());
      });
  }

  onPageChange(event: { page: number; pageSize: number }): void {
    this.page = event.page;
    this.pageSize = event.pageSize;
    this.loadLevels();
  }
}
