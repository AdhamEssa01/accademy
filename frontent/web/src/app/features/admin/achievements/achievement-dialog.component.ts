import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AchievementDto } from '../../../core/api/achievements.api';

export interface AchievementDialogData {
  achievement?: AchievementDto;
}

@Component({
  selector: 'app-achievement-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  templateUrl: './achievement-dialog.component.html',
})
export class AchievementDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<AchievementDialogComponent>);
  readonly data = inject(MAT_DIALOG_DATA) as AchievementDialogData;

  readonly form = this.fb.nonNullable.group({
    title: [this.data.achievement?.title ?? '', [Validators.required, Validators.maxLength(200)]],
    description: [this.data.achievement?.description ?? ''],
    tag: [this.data.achievement?.tag ?? ''],
    imageUrl: [this.data.achievement?.imageUrl ?? ''],
  });

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    this.dialogRef.close({
      ...raw,
      description: raw.description || null,
      tag: raw.tag || null,
      imageUrl: raw.imageUrl || null,
    });
  }
}
