import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { SaleService } from '../sale.service';

@Component({
  selector: 'app-serial-trace',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ReactiveFormsModule
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
    if (!val) return;

    this.hasSearched.set(true);
    this.saleService.getSerialTrace(val).subscribe();
  }
}
