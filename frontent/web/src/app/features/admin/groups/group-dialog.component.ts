import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { GroupDto } from '../../../core/api/groups.api';

export interface GroupDialogData {
  group?: GroupDto;
}

@Component({
  selector: 'app-group-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  templateUrl: './group-dialog.component.html',
})
export class GroupDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<GroupDialogComponent>);
  readonly data = inject(MAT_DIALOG_DATA) as GroupDialogData;

  readonly form = this.fb.nonNullable.group({
    name: [this.data.group?.name ?? '', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    programId: [this.data.group?.programId ?? '', Validators.required],
    courseId: [this.data.group?.courseId ?? '', Validators.required],
    levelId: [this.data.group?.levelId ?? '', Validators.required],
    instructorUserId: [this.data.group?.instructorUserId ?? ''],
  });

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    this.dialogRef.close({
      ...raw,
      instructorUserId: raw.instructorUserId || null,
    });
  }
}
