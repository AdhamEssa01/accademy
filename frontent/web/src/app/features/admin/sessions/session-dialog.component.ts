import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { SessionDto } from '../../../core/api/groups.api';

export interface SessionDialogData {
  session?: SessionDto;
}

@Component({
  selector: 'app-session-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  templateUrl: './session-dialog.component.html',
})
export class SessionDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<SessionDialogComponent>);
  readonly data = inject(MAT_DIALOG_DATA) as SessionDialogData;

  readonly form = this.fb.nonNullable.group({
    groupId: [this.data.session?.groupId ?? '', Validators.required],
    instructorUserId: [this.data.session?.instructorUserId ?? '', Validators.required],
    startsAtUtc: [this.data.session?.startsAtUtc ?? '', Validators.required],
    durationMinutes: [this.data.session?.durationMinutes ?? 60, Validators.min(15)],
    notes: [this.data.session?.notes ?? ''],
  });

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.dialogRef.close(this.form.getRawValue());
  }
}
