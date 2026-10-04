import path from 'node:path';
import { pathToFileURL } from 'node:url';
import { mkdir } from 'node:fs/promises';

const playwrightPath = process.env.PLAYWRIGHT_CORE_PATH
  ?? 'C:/Users/lukin/AppData/Local/Temp/locadora-playwright/node_modules/playwright-core/index.mjs';
const { chromium } = await import(pathToFileURL(playwrightPath).href);

const baseUrl = process.env.API_URL ?? 'http://127.0.0.1:5190';
const outputDirectory = path.resolve('Etapa3/evidencias');
const edgePath = 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe';
await mkdir(outputDirectory, { recursive: true });

const browser = await chromium.launch({
  executablePath: edgePath,
  headless: true,
  args: ['--disable-features=msEdgeSidebarV2', '--hide-scrollbars']
});
const page = await browser.newPage({ viewport: { width: 1600, height: 1000 }, deviceScaleFactor: 1 });

async function loadSwagger() {
  await page.goto(`${baseUrl}/swagger/index.html`, { waitUntil: 'networkidle' });
  await page.locator('.swagger-ui .info').waitFor();
  await page.locator('.models').evaluate(element => element.style.display = 'none').catch(() => {});
}

async function findOperation(tag, route, method) {
  await loadSwagger();
  const blocks = page.locator(`.opblock.opblock-${method.toLowerCase()}`);
  await blocks.first().waitFor();
  for (let index = 0; index < await blocks.count(); index++) {
    const block = blocks.nth(index);
    const displayedRoute = (await block.locator('.opblock-summary-path').innerText()).trim();
    if (displayedRoute === route) return block;
  }
  throw new Error(`Operação não encontrada: ${method} ${route}`);
}

async function executeOperation({
  tag, route, method, parameters = {}, body, expectedStatus, screenshot
}) {
  const operation = await findOperation(tag, route, method);
  await operation.locator('.opblock-summary').click();
  await operation.locator('button.try-out__btn').click();

  for (const [name, value] of Object.entries(parameters)) {
    const row = operation.locator(`tr[data-param-name="${name}"]`);
    await row.locator('input').fill(String(value));
  }
  if (body !== undefined) {
    await operation.locator('textarea.body-param__text').fill(JSON.stringify(body, null, 2));
  }

  await operation.locator('button.execute').click();
  const statusCell = operation.locator('.live-responses-table tbody .response-col_status').first();
  await statusCell.waitFor({ timeout: 15000 });
  await page.waitForFunction(
    ({ selector, expected }) => document.querySelector(selector)?.textContent?.trim().startsWith(expected),
    { selector: '.opblock.is-open .live-responses-table tbody .response-col_status', expected: String(expectedStatus) },
    { timeout: 15000 }
  ).catch(async () => {
    const actual = (await statusCell.innerText()).trim();
    if (!actual.startsWith(String(expectedStatus))) throw new Error(`${method} ${route}: esperado ${expectedStatus}, obtido ${actual}`);
  });

  await operation.scrollIntoViewIfNeeded();
  await operation.screenshot({ path: path.join(outputDirectory, screenshot), animations: 'disabled' });
  console.log(`${screenshot}: ${method} ${route} -> ${expectedStatus}`);
  return operation;
}

try {
  await loadSwagger();
  await page.screenshot({
    path: path.join(outputDirectory, '01-swagger-visao-geral.png'),
    fullPage: true,
    animations: 'disabled'
  });
  console.log('01-swagger-visao-geral.png: documentação carregada');

  await executeOperation({
    tag: 'Alugueis', route: '/api/alugueis', method: 'GET', expectedStatus: 200,
    screenshot: '02-get-alugueis-200.png'
  });

  const evidenceName = `Marca Evidência Swagger ${Date.now()}`;
  const postOperation = await executeOperation({
    tag: 'Fabricantes', route: '/api/fabricantes', method: 'POST', expectedStatus: 201,
    body: { nome: evidenceName, paisOrigem: 'Brasil' },
    screenshot: '03-post-fabricante-201.png'
  });
  const responseText = await postOperation.locator('.live-responses-table .response-col_description pre').first().innerText();
  const evidenceManufacturerId = JSON.parse(responseText).id;

  await executeOperation({
    tag: 'Fabricantes', route: '/api/fabricantes/{id}', method: 'PUT', expectedStatus: 204,
    parameters: { id: evidenceManufacturerId },
    body: { nome: 'Marca Evidência Atualizada', paisOrigem: 'Brasil' },
    screenshot: '04-put-fabricante-204.png'
  });

  await executeOperation({
    tag: 'Fabricantes', route: '/api/fabricantes/{id}', method: 'DELETE', expectedStatus: 204,
    parameters: { id: evidenceManufacturerId }, screenshot: '05-delete-fabricante-204.png'
  });

  await executeOperation({
    tag: 'Clientes', route: '/api/clientes', method: 'POST', expectedStatus: 400,
    body: {}, screenshot: '06-validacao-400.png'
  });

  await executeOperation({
    tag: 'Fabricantes', route: '/api/fabricantes/{id}', method: 'GET', expectedStatus: 404,
    parameters: { id: 99999 }, screenshot: '07-nao-encontrado-404.png'
  });

  await executeOperation({
    tag: 'Fabricantes', route: '/api/fabricantes', method: 'POST', expectedStatus: 409,
    body: { nome: 'Volkswagen Brasil', paisOrigem: 'Alemanha' },
    screenshot: '08-conflito-409.png'
  });

  await executeOperation({
    tag: 'Filtros', route: '/api/filtros/veiculos-por-fabricante', method: 'GET', expectedStatus: 200,
    parameters: { nome: 'Volkswagen' }, screenshot: '09-filtro1-fabricante-200.png'
  });
  await executeOperation({
    tag: 'Filtros', route: '/api/filtros/veiculos-por-categoria', method: 'GET', expectedStatus: 200,
    parameters: { nome: 'SUV', quilometragemMaxima: 50000 }, screenshot: '10-filtro2-categoria-200.png'
  });
  await executeOperation({
    tag: 'Filtros', route: '/api/filtros/alugueis-por-cliente', method: 'GET', expectedStatus: 200,
    parameters: { termo: 'Maria' }, screenshot: '11-filtro3-cliente-200.png'
  });
  await executeOperation({
    tag: 'Filtros', route: '/api/filtros/alugueis-por-periodo', method: 'GET', expectedStatus: 200,
    parameters: { inicio: '2026-10-01T00:00:00', fim: '2026-10-10T00:00:00' },
    screenshot: '12-filtro4-periodo-200.png'
  });
  await executeOperation({
    tag: 'Filtros', route: '/api/filtros/clientes-sem-aluguel', method: 'GET', expectedStatus: 200,
    screenshot: '13-filtro5-left-join-200.png'
  });
} finally {
  await browser.close();
}
