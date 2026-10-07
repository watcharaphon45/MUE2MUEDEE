import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CatalogService } from '../catalog.service';
import { PublicCatalogItem, PublicStoreInfo } from '../catalog.models';

@Component({
  selector: 'app-storefront',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './storefront.component.html',
  styleUrl: './storefront.component.css'
})
export class StorefrontComponent implements OnInit {
  protected readonly catalogService = inject(CatalogService);
  private readonly route = inject(ActivatedRoute);

  readonly storeCode = signal<string>('');
  readonly searchQuery = signal<string>('');
  readonly selectedBrandId = signal<string>('');
  readonly selectedCategoryId = signal<string>('');
  readonly selectedGradeId = signal<string>('');
  readonly sortBy = signal<string>('newest');

  // Modals
  readonly selectedItem = signal<PublicCatalogItem | null>(null);
  readonly isInquireModalOpen = signal<boolean>(false);
  readonly isContactStoreModalOpen = signal<boolean>(false);
  readonly copiedMessage = signal<boolean>(false);

  readonly store = computed<PublicStoreInfo | null>(() => this.catalogService.store());
  readonly items = computed<PublicCatalogItem[]>(() => this.catalogService.items());
  readonly filterMeta = computed(() => this.catalogService.filterMeta());
  readonly isLoading = computed(() => this.catalogService.isLoading());
  readonly errorMessage = computed(() => this.catalogService.errorMessage());

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const code = params.get('storeCode');
      if (code) {
        this.storeCode.set(code);
        this.loadCatalog();
      }
    });
  }

  loadCatalog(): void {
    const code = this.storeCode();
    if (!code) return;

    this.catalogService.loadStoreCatalog(code, {
      search: this.searchQuery() || undefined,
      brandId: this.selectedBrandId() || undefined,
      categoryId: this.selectedCategoryId() || undefined,
      conditionGradeId: this.selectedGradeId() || undefined,
      sortBy: this.sortBy(),
      pageSize: 50
    }).subscribe();
  }

  onSearchSubmit(): void {
    this.loadCatalog();
  }

  onFilterChange(): void {
    this.loadCatalog();
  }

  clearFilters(): void {
    this.searchQuery.set('');
    this.selectedBrandId.set('');
    this.selectedCategoryId.set('');
    this.selectedGradeId.set('');
    this.sortBy.set('newest');
    this.loadCatalog();
  }

  openItemDetail(item: PublicCatalogItem): void {
    this.selectedItem.set(item);
  }

  closeItemDetail(): void {
    this.selectedItem.set(null);
  }

  openInquireModal(item?: PublicCatalogItem): void {
    if (item) {
      this.selectedItem.set(item);
    }
    this.copiedMessage.set(false);
    this.isInquireModalOpen.set(true);
  }

  closeInquireModal(): void {
    this.isInquireModalOpen.set(false);
  }

  openContactStoreModal(): void {
    this.isContactStoreModalOpen.set(true);
  }

  closeContactStoreModal(): void {
    this.isContactStoreModalOpen.set(false);
  }

  getInquiryText(): string {
    const item = this.selectedItem();
    const st = this.store();
    if (!item) {
      return `สวัสดีครับ สนใจสอบถามสินค้าจากร้าน ${st?.name ?? ''} ครับ`;
    }
    return `สวัสดีครับ สนใจสินค้า: ${item.brandName} ${item.modelName} (สภาพ ${item.gradeName}) ราคา ฿${item.sellingPrice.toLocaleString()} รหัส ${item.sku} จากร้าน ${st?.name ?? ''} ครับ ไม่ทราบว่ายังมีสินค้าอยู่ไหมครับ?`;
  }

  copyInquiryText(): void {
    const text = this.getInquiryText();
    if (navigator?.clipboard) {
      navigator.clipboard.writeText(text).then(() => {
        this.copiedMessage.set(true);
        setTimeout(() => this.copiedMessage.set(false), 3000);
      });
    } else {
      // Fallback
      const textarea = document.createElement('textarea');
      textarea.value = text;
      document.body.appendChild(textarea);
      textarea.select();
      document.execCommand('copy');
      document.body.removeChild(textarea);
      this.copiedMessage.set(true);
      setTimeout(() => this.copiedMessage.set(false), 3000);
    }
  }

  getGradeBadgeClass(gradeCode: string): string {
    const code = gradeCode?.toUpperCase();
    if (code === 'A+' || code === 'A') return 'bg-emerald-50 text-emerald-700 border-emerald-200';
    if (code === 'B+' || code === 'B') return 'bg-sky-50 text-sky-700 border-sky-200';
    if (code === 'C+' || code === 'C') return 'bg-amber-50 text-amber-700 border-amber-200';
    return 'bg-slate-50 text-slate-700 border-slate-200';
  }
}
