import { Component, inject } from '@angular/core';
import { NgFor } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { CourseDto, ProgramDto } from '../../../core/api/programs.api';

export interface CourseDialogData {
  course?: CourseDto;
  programs: ProgramDto[];
}

@Component({
  selector: 'app-course-dialog',
  standalone: true,
  imports: [
    NgFor,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  templateUrl: './course-dialog.component.html',
})
export class CourseDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<CourseDialogComponent>);
  readonly data = inject(MAT_DIALOG_DATA) as CourseDialogData;

  readonly form = this.fb.nonNullable.group({
    programId: [this.data.course?.programId ?? '', Validators.required],
    name: [this.data.course?.name ?? '', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    description: [this.data.course?.description ?? ''],
  });

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.dialogRef.close(this.form.getRawValue());
  }
}
