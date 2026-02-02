import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { StudentDto } from '../../../core/api/students.api';

export interface StudentDialogData {
  student?: StudentDto;
}

@Component({
  selector: 'app-student-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  templateUrl: './student-dialog.component.html',
})
export class StudentDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<StudentDialogComponent>);
  readonly data = inject(MAT_DIALOG_DATA) as StudentDialogData;

  readonly form = this.fb.nonNullable.group({
    fullName: [this.data.student?.fullName ?? '', [Validators.required, Validators.maxLength(200)]],
    dateOfBirth: [this.data.student?.dateOfBirth ?? ''],
    notes: [this.data.student?.notes ?? ''],
  });

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    this.dialogRef.close({
      ...raw,
      dateOfBirth: raw.dateOfBirth || null,
      notes: raw.notes || null,
    });
  }
}
