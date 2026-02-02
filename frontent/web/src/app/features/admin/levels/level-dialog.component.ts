import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { CourseDto, LevelDto } from '../../../core/api/programs.api';
import { NgFor } from '@angular/common';

export interface LevelDialogData {
  level?: LevelDto;
  courses: CourseDto[];
}

@Component({
  selector: 'app-level-dialog',
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
  templateUrl: './level-dialog.component.html',
})
export class LevelDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<LevelDialogComponent>);
  readonly data = inject(MAT_DIALOG_DATA) as LevelDialogData;

  readonly form = this.fb.nonNullable.group({
    courseId: [this.data.level?.courseId ?? '', Validators.required],
    name: [this.data.level?.name ?? '', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    sortOrder: [this.data.level?.sortOrder ?? 0, Validators.min(0)],
  });

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.dialogRef.close(this.form.getRawValue());
  }
}
