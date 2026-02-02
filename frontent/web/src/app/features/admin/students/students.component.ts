import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { StudentsApi, StudentDto } from '../../../core/api/students.api';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { SkeletonCardsComponent } from '../../../shared/skeleton/skeleton-cards.component';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';
import { StudentDialogComponent } from './student-dialog.component';

@Component({
  selector: 'app-students',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgFor, NgIf, RouterLink, MatCardModule, MatButtonModule, MatIconModule, PaginationComponent, SkeletonCardsComponent],
  templateUrl: './students.component.html',
  styleUrl: './students.component.scss',
})
export class StudentsComponent implements OnInit {
  private readonly api = inject(StudentsApi);
  private readonly dialog = inject(MatDialog);
  private readonly confirm = inject(ConfirmDialogService);

  students: StudentDto[] = [];
  page = 1;
  pageSize = 10;
  total = 0;
  isLoading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.api.list(this.page, this.pageSize).subscribe({
      next: (response) => {
        this.students = response.items;
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

  openCreate(): void {
    const ref = this.dialog.open(StudentDialogComponent, { width: '460px' });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.create(result).subscribe(() => this.load());
    });
  }

  openEdit(student: StudentDto): void {
    const ref = this.dialog.open(StudentDialogComponent, {
      width: '460px',
      data: { student },
    });
    ref.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }
      this.api.update(student.id, result).subscribe(() => this.load());
    });
  }

  delete(student: StudentDto): void {
    this.confirm
      .confirm({
        title: 'Delete student',
        message: `Delete ${student.fullName}?`,
        confirmText: 'Delete',
      })
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }
        this.api.delete(student.id).subscribe(() => this.load());
      });
  }

  onPageChange(event: { page: number; pageSize: number }): void {
    this.page = event.page;
    this.pageSize = event.pageSize;
    this.load();
  }
}
