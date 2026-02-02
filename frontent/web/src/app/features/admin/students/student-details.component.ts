import { ChangeDetectionStrategy, Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { StudentsApi, StudentDto } from '../../../core/api/students.api';

@Component({
  selector: 'app-student-details',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgIf, MatCardModule, MatButtonModule],
  templateUrl: './student-details.component.html',
  styleUrl: './student-details.component.scss',
})
export class StudentDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(StudentsApi);

  student?: StudentDto;
  isUploading = false;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.load(id);
    }
  }

  load(id: string): void {
    this.api.get(id).subscribe((student) => (this.student = student));
  }

  onFileSelected(event: Event): void {
    const id = this.student?.id;
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!id || !file) {
      return;
    }

    this.isUploading = true;
    this.api.uploadPhoto(id, file).subscribe({
      next: (student) => {
        this.student = student;
        this.isUploading = false;
      },
      error: () => {
        this.isUploading = false;
      },
    });
  }
}
