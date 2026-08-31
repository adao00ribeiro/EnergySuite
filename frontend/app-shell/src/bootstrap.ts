import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

bootstrapApplication(App, appConfig).catch((err) => {
  console.error('Falha ao inicializar a aplicação:', err);
  showBootstrapError();
});

function showBootstrapError() {
  const appEl = document.querySelector('app-root');
  if (!appEl) return;

  const initError = sessionStorage.getItem('energysuite_init_error');
  appEl.innerHTML = `
    <div style="display:flex;align-items:center;justify-content:center;min-height:100vh;padding:24px;box-sizing:border-box;background:#0F172A;color:#F8FAFC;font-family:'Inter',sans-serif;">
      <div style="text-align:center;max-width:420px;background:#1E293B;border:1px solid #334155;border-radius:16px;padding:40px 32px;box-shadow:0 10px 15px rgba(0,0,0,0.15);">
        <div style="width:72px;height:72px;border-radius:50%;margin:0 auto 16px;display:flex;align-items:center;justify-content:center;background:rgba(220,38,38,0.15);font-size:40px;" aria-hidden="true">&#9888;</div>
        <h1 style="margin:0 0 12px;font-size:1.4rem;font-weight:600;">Serviço Indisponível</h1>
        <p style="margin:0 0 20px;color:#94A3B8;line-height:1.6;">
          ${initError || 'Não foi possível carregar a aplicação. Verifique sua conexão com os serviços.'}
        </p>
        <button id="retry-bootstrap" style="background:#0369A1;color:#fff;border:none;border-radius:10px;padding:12px 24px;font-weight:600;cursor:pointer;font-family:inherit;font-size:14px;">Tentar Novamente</button>
      </div>
    </div>
  `;

  const retryBtn = document.getElementById('retry-bootstrap');
  if (retryBtn) {
    retryBtn.addEventListener('click', () => {
      sessionStorage.removeItem('energysuite_init_error');
      window.location.reload();
    });
  }
}
