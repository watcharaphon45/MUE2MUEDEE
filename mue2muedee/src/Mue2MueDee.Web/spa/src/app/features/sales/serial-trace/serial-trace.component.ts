import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { SaleService } from '../sale.service';
import { TextboxComponent } from '../../../shared/components/textbox/textbox.component';

@Component({
  selector: 'app-serial-trace',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule,
    TextboxComponent
  ],
  templateUrl: './serial-trace.component.html',
  styleUrl: './serial-trace.component.css'
})
export class SerialTraceComponent implements OnInit {
  protected readonly saleService = inject(SaleService);
  private readonly route = inject(ActivatedRoute);

  serialControl = new FormControl('', [Validators.required]);
  hasSearched = signal<boolean>(false);

  ngOnInit(): void {
    const sn = this.route.snapshot.queryParamMap.get('sn');
    if (sn) {
      this.serialControl.setValue(sn);
      this.searchTrace();
    }
  }

  searchTrace(): void {
    const val = this.serialControl.value?.trim();
    if (!val) {
      this.serialControl.setErrors({ required: true });
      this.serialControl.markAsTouched();
      return;
    }

    this.hasSearched.set(true);
    this.saleService.getSerialTrace(val).subscribe();
  }
}
