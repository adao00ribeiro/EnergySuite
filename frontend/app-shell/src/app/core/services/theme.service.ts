import { Injectable, signal, effect, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private platformId = inject(PLATFORM_ID);
  
  // Sinal que guarda se o tema atual é escuro ou não
  public isDark = signal<boolean>(false);

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      // Tenta recuperar a preferência do localStorage
      const savedTheme = localStorage.getItem('theme');
      
      if (savedTheme) {
        this.isDark.set(savedTheme === 'dark');
      } else {
        // Se não tiver salvo, pega a preferência do sistema operacional
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        this.isDark.set(prefersDark);
      }

      // Effect que reage a mudanças no signal
      effect(() => {
        const dark = this.isDark();
        localStorage.setItem('theme', dark ? 'dark' : 'light');
        
        if (dark) {
          document.body.classList.add('dark-theme');
        } else {
          document.body.classList.remove('dark-theme');
        }
      });
    }
  }

  public toggleTheme(): void {
    this.isDark.update(current => !current);
  }
}
