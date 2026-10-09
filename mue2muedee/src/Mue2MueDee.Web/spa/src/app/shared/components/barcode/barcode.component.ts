import {
  Component,
  ElementRef,
  ViewChild,
  input,
  effect,
  ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import JsBarcode from 'jsbarcode';

@Component({
  selector: 'app-barcode',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './barcode.component.html',
  styleUrl: './barcode.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BarcodeComponent {
  @ViewChild('barcodeSvg', { static: true }) barcodeSvgRef!: ElementRef<SVGSVGElement>;

  value = input<string>('');
  format = input<string>('CODE128');
  width = input<number>(1.6);
  height = input<number>(40);
  displayValue = input<boolean>(true);
  fontSize = input<number>(11);
  margin = input<number>(4);
  background = input<string>('#ffffff');
  lineColor = input<string>('#000000');

  hasError = false;

  constructor() {
    effect(() => {
      const val = this.value();
      const fmt = this.format();
      const w = this.width();
      const h = this.height();
      const showText = this.displayValue();
      const fSize = this.fontSize();
      const m = this.margin();
      const bg = this.background();
      const color = this.lineColor();

      this.renderBarcode(val, fmt, w, h, showText, fSize, m, bg, color);
    });
  }

  private renderBarcode(
    val: string,
    fmt: string,
    w: number,
    h: number,
    showText: boolean,
    fSize: number,
    m: number,
    bg: string,
    color: string
  ): void {
    if (!this.barcodeSvgRef?.nativeElement || !val) {
      this.hasError = false;
      return;
    }

    try {
      JsBarcode(this.barcodeSvgRef.nativeElement, val, {
        format: fmt,
        width: w,
        height: h,
        displayValue: showText,
        fontSize: fSize,
        font: 'monospace',
        fontOptions: 'bold',
        textAlign: 'center',
        textPosition: 'bottom',
        textMargin: 2,
        margin: m,
        background: bg,
        lineColor: color,
        valid: (valid) => {
          this.hasError = !valid;
        }
      });
      this.hasError = false;

      // Ensure SVG has viewBox for responsive scaling without clipping
      const svg = this.barcodeSvgRef.nativeElement;
      const widthAttr = svg.getAttribute('width');
      const heightAttr = svg.getAttribute('height');
      if (widthAttr && heightAttr) {
        svg.setAttribute('viewBox', `0 0 ${widthAttr} ${heightAttr}`);
        svg.setAttribute('preserveAspectRatio', 'xMidYMid meet');
      }
    } catch {
      this.hasError = true;
    }
  }
}
