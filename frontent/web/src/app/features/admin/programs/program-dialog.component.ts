import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ProgramDto } from '../../../core/api/programs.api';

export interface ProgramDialogData {
  program?: ProgramDto;
}

@Component({
  selector: 'app-program-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  templateUrl: './program-dialog.component.html',
})
export class ProgramDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ProgramDialogComponent>);
  readonly data = inject(MAT_DIALOG_DATA) as ProgramDialogData;

  readonly form = this.fb.nonNullable.group({
    name: [this.data.program?.name ?? '', [Validators.required, Validators.minLength(2), Validators.maxLength(150)]],
    description: [this.data.program?.description ?? ''],
  });

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.dialogRef.close(this.form.getRawValue());
  }
}
