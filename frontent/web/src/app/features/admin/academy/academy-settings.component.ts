import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AcademyApi } from '../../../core/api/academy.api';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-academy-settings',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule],
  templateUrl: './academy-settings.component.html',
  styleUrl: './academy-settings.component.scss',
})
export class AcademySettingsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(AcademyApi);
  private readonly snackBar = inject(MatSnackBar);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
  });

  isSaving = false;

  ngOnInit(): void {
    this.api.getMine().subscribe((academy) => {
      this.form.patchValue({ name: academy.name });
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.api.updateMine(this.form.getRawValue()).subscribe({
      next: () => {
        this.isSaving = false;
        this.snackBar.open('Academy updated.', 'Dismiss', { duration: 3000 });
      },
      error: () => {
        this.isSaving = false;
      },
    });
  }
}
