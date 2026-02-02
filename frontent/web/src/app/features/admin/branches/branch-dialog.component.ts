import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { BranchDto } from '../../../core/api/branches.api';

export interface BranchDialogData {
  branch?: BranchDto;
}

@Component({
  selector: 'app-branch-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  templateUrl: './branch-dialog.component.html',
})
export class BranchDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<BranchDialogComponent>);
  readonly data = inject(MAT_DIALOG_DATA) as BranchDialogData;

  readonly form = this.fb.nonNullable.group({
    name: [this.data.branch?.name ?? '', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    address: [this.data.branch?.address ?? ''],
  });

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.dialogRef.close(this.form.getRawValue());
  }
}
