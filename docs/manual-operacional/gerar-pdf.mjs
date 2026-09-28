// Gera o PDF do manual operacional a partir de manual.html.
// Requer Playwright instalado (`npm i -D playwright` ou disponível globalmente).
// Uso: node docs/manual-operacional/gerar-pdf.mjs
import { chromium } from "playwright";
import path from "path";
import { fileURLToPath } from "url";

const DIR = path.dirname(fileURLToPath(import.meta.url));
const HTML = path.join(DIR, "manual.html");
const OUT = path.join(DIR, "Manual-Operacional-Mesada-App.pdf");

const browser = await chromium.launch();
const page = await browser.newPage();
await page.goto(`file://${HTML}`, { waitUntil: "networkidle" });
await page.pdf({
  path: OUT,
  format: "A4",
  printBackground: true,
  displayHeaderFooter: true,
  margin: { top: "10mm", bottom: "14mm", left: "0mm", right: "0mm" },
  headerTemplate: `<div></div>`,
  footerTemplate: `
    <div style="width:100%; font-size:9px; color:#8a9490; padding:0 18mm; display:flex; justify-content:space-between; font-family: -apple-system, Arial, sans-serif;">
      <span>Mesada App — Manual Operacional</span>
      <span><span class="pageNumber"></span> / <span class="totalPages"></span></span>
    </div>`,
});
console.log("PDF gerado em", OUT);
await browser.close();
