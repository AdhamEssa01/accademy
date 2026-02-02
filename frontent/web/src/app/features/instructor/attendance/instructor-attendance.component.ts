import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { AttendanceApi, AttendanceItemRequest, AttendanceStatus } from '../../../core/api/attendance.api';

@Component({
  selector: 'app-instructor-attendance',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FormsModule, NgFor, NgIf, MatCardModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  templateUrl: './instructor-attendance.component.html',
  styleUrl: './instructor-attendance.component.scss',
})
export class InstructorAttendanceComponent {
  private readonly api = inject(AttendanceApi);

  sessionId = '';
  items: AttendanceItemRequest[] = [];
  statuses: AttendanceStatus[] = ['Present', 'Absent', 'Late', 'Excused'];
  isSubmitting = false;

  load(): void {
    if (!this.sessionId) {
      return;
    }
    this.api.getSessionAttendance(this.sessionId).subscribe((records) => {
      this.items = records.map((item) => ({
        studentId: item.studentId,
        status: item.status,
        reason: item.reason ?? '',
        note: item.note ?? '',
      }));
    });
  }

  submit(): void {
    if (!this.sessionId || this.items.length === 0) {
      return;
    }
    this.isSubmitting = true;
    this.api.submitSessionAttendance(this.sessionId, { items: this.items }).subscribe({
      next: () => {
        this.isSubmitting = false;
      },
      error: () => {
        this.isSubmitting = false;
      },
    });
  }
}
