import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { AttendanceApi, AttendanceItemRequest, AttendanceRecordDto, AttendanceStatus } from '../../../core/api/attendance.api';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';

@Component({
  selector: 'app-admin-attendance',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    FormsModule,
    NgFor,
    NgIf,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    PaginationComponent,
  ],
  templateUrl: './admin-attendance.component.html',
  styleUrl: './admin-attendance.component.scss',
})
export class AdminAttendanceComponent {
  private readonly api = inject(AttendanceApi);

  sessionId = '';
  sessionItems: AttendanceItemRequest[] = [];
  statuses: AttendanceStatus[] = ['Present', 'Absent', 'Late', 'Excused'];
  isSubmitting = false;

  reportFilters = {
    groupId: '',
    studentId: '',
    from: '',
    to: '',
    status: '' as AttendanceStatus | '',
  };
  reportPage = 1;
  reportPageSize = 10;
  reportTotal = 0;
  reportItems: AttendanceRecordDto[] = [];

  loadSessionAttendance(): void {
    if (!this.sessionId) {
      return;
    }
    this.api.getSessionAttendance(this.sessionId).subscribe((items) => {
      this.sessionItems = items.map((item) => ({
        studentId: item.studentId,
        status: item.status,
        reason: item.reason ?? '',
        note: item.note ?? '',
      }));
    });
  }

  submitAttendance(): void {
    if (!this.sessionId || this.sessionItems.length === 0) {
      return;
    }
    this.isSubmitting = true;
    this.api
      .submitSessionAttendance(this.sessionId, { items: this.sessionItems })
      .subscribe({
        next: () => {
          this.isSubmitting = false;
        },
        error: () => {
          this.isSubmitting = false;
        },
      });
  }

  loadReport(): void {
    this.api
      .listAttendance({
        groupId: this.reportFilters.groupId || null,
        studentId: this.reportFilters.studentId || null,
        from: this.reportFilters.from || null,
        to: this.reportFilters.to || null,
        status: this.reportFilters.status || null,
        page: this.reportPage,
        pageSize: this.reportPageSize,
      })
      .subscribe((response) => {
        this.reportItems = response.items;
        this.reportPage = response.page;
        this.reportPageSize = response.pageSize;
        this.reportTotal = response.total;
      });
  }

  onReportPageChange(event: { page: number; pageSize: number }): void {
    this.reportPage = event.page;
    this.reportPageSize = event.pageSize;
    this.loadReport();
  }
}
