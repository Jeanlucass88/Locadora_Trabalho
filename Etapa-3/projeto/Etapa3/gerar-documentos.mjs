import { readFile, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { pathToFileURL } from 'node:url';

const toolsRoot = 'C:/Users/lukin/AppData/Local/Temp/locadora-playwright/node_modules';
const { marked } = await import(pathToFileURL(`${toolsRoot}/marked/lib/marked.esm.js`).href);
const { chromium } = await import(pathToFileURL(`${toolsRoot}/playwright-core/index.mjs`).href);

const directory = path.resolve('Etapa3');
const documents = [
  ['RELATORIO_TESTES.md', 'RELATORIO_TESTES.html', 'RELATORIO_TESTES.pdf', 'Relatório de Testes — Etapa 3'],
  ['DOCUMENTACAO_ENDPOINTS.md', 'DOCUMENTACAO_ENDPOINTS.html', 'DOCUMENTACAO_ENDPOINTS.pdf', 'Documentação dos Endpoints']
];

const css = `
  @page { size: A4; margin: 15mm 13mm 16mm; }
  * { box-sizing: border-box; }
  body { font: 10.5pt/1.45 "Segoe UI", Arial, sans-serif; color: #20242a; max-width: 1100px; margin: 30px auto; padding: 0 24px; }
  h1 { color: #17365d; border-bottom: 3px solid #5b9bd5; padding-bottom: 8px; }
  h2 { color: #1f4e79; border-bottom: 1px solid #bdd7ee; padding-bottom: 4px; margin-top: 28px; }
  h3 { color: #2f5597; break-after: avoid; }
  table { width: 100%; border-collapse: collapse; margin: 12px 0 20px; font-size: 9pt; break-inside: auto; }
  tr { break-inside: avoid; }
  th { background: #d9eaf7; text-align: left; }
  th, td { border: 1px solid #9fbad0; padding: 5px 7px; vertical-align: top; }
  code { font-family: Consolas, monospace; background: #eef2f5; border-radius: 3px; padding: 1px 3px; }
  pre { background: #20242a; color: #f4f4f4; padding: 12px; border-radius: 5px; overflow-wrap: anywhere; white-space: pre-wrap; }
  img { display: block; max-width: 100%; max-height: 245mm; object-fit: contain; margin: 10px auto 24px; border: 1px solid #bbb; break-inside: avoid; }
  blockquote { border-left: 4px solid #5b9bd5; padding-left: 12px; color: #555; }
  @media print { body { margin: 0; padding: 0; max-width: none; } h2, h3 { break-after: avoid; } a { color: inherit; text-decoration: none; } }
`;

const browser = await chromium.launch({
  executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
  headless: true
});

try {
  for (const [markdownName, htmlName, pdfName, title] of documents) {
    const markdown = await readFile(path.join(directory, markdownName), 'utf8');
    const body = marked.parse(markdown);
    const html = `<!doctype html><html lang="pt-BR"><head><meta charset="utf-8"><title>${title}</title><style>${css}</style></head><body>${body}</body></html>`;
    const htmlPath = path.join(directory, htmlName);
    await writeFile(htmlPath, html, 'utf8');

    const page = await browser.newPage();
    await page.goto(pathToFileURL(htmlPath).href, { waitUntil: 'networkidle' });
    await page.pdf({
      path: path.join(directory, pdfName),
      format: 'A4',
      printBackground: true,
      margin: { top: '15mm', right: '13mm', bottom: '16mm', left: '13mm' }
    });
    await page.close();
    console.log(`${htmlName} e ${pdfName} gerados.`);
  }
} finally {
  await browser.close();
}
