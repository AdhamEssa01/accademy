import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { CmsApi } from '../../../core/api/cms.api';

@Component({
  selector: 'app-about',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgIf, NgFor, MatCardModule],
  templateUrl: './about.component.html',
  styleUrl: './about.component.scss',
})
export class AboutComponent implements OnInit {
  title = 'About Academy';
  sections: Array<{ title: string; body: string }> = [];

  constructor(private readonly cmsApi: CmsApi) {}

  ngOnInit(): void {
    this.cmsApi.getPage('about').subscribe((page) => {
      if (page?.title) {
        this.title = page.title;
      }
      const cmsSections = page?.sections ?? [];
      if (cmsSections.length > 0) {
        this.sections = cmsSections.map((section) => ({
          title: section.type,
          body: section.jsonContent ?? '',
        }));
      }
    });
  }
}
