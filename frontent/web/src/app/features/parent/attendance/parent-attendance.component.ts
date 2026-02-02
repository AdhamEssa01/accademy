import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AttendanceApi, AttendanceRecordDto } from '../../../core/api/attendance.api';
import { SkeletonCardsComponent } from '../../../shared/skeleton/skeleton-cards.component';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';

@Component({
  selector: 'app-parent-attendance',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    NgFor,
    NgIf,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    PaginationComponent,
    SkeletonCardsComponent,
  ],
  templateUrl: './parent-attendance.component.html',
  styleUrl: './parent-attendance.component.scss',
})
export class ParentAttendanceComponent implements OnInit {
  private readonly api = inject(AttendanceApi);

  from = '';
  to = '';
  page = 1;
  pageSize = 10;
  total = 0;
  records: AttendanceRecordDto[] = [];
  isLoading = false;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.api
      .listMyChildrenAttendance(this.page, this.pageSize, this.from || undefined, this.to || undefined)
      .subscribe({
        next: (response) => {
          this.records = response.items;
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

  onPageChange(event: { page: number; pageSize: number }): void {
    this.page = event.page;
    this.pageSize = event.pageSize;
    this.load();
  }
}
