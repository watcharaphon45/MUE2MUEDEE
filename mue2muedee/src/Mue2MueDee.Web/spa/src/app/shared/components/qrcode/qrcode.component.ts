import {
  Component,
  input,
  effect,
  ChangeDetectionStrategy,
  inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import QRCode from 'qrcode';

@Component({
  selector: 'app-qrcode',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './qrcode.component.html',
  styleUrl: './qrcode.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class QrcodeComponent {
  private sanitizer = inject(DomSanitizer);

  value = input<string>('');
  size = input<number>(72);
  margin = input<number>(1);
  errorCorrectionLevel = input<'L' | 'M' | 'Q' | 'H'>('M');
  darkColor = input<string>('#000000');
  lightColor = input<string>('#ffffff');

  svgHtml: SafeHtml | null = null;
  hasError = false;

  constructor() {
    effect(() => {
      const val = this.value();
      const s = this.size();
      const m = this.margin();
      const ecl = this.errorCorrectionLevel();
      const dark = this.darkColor();
      const light = this.lightColor();

      this.renderQrCode(val, s, m, ecl, dark, light);
    });
  }

  private async renderQrCode(
    val: string,
    s: number,
    m: number,
    ecl: 'L' | 'M' | 'Q' | 'H',
    dark: string,
    light: string
  ): Promise<void> {
    if (!val) {
      this.svgHtml = null;
      this.hasError = false;
      return;
    }

    try {
      const svgString = await QRCode.toString(val, {
        type: 'svg',
        width: s,
        margin: m,
        errorCorrectionLevel: ecl,
        color: {
          dark: dark,
          light: light
        }
      });
      this.svgHtml = this.sanitizer.bypassSecurityTrustHtml(svgString);
      this.hasError = false;
    } catch {
      this.hasError = true;
      this.svgHtml = null;
    }
  }
}
