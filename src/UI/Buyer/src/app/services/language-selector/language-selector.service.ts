import { Injectable } from '@angular/core'
import { TranslateService } from '@ngx-translate/core'
import { TokenHelperService } from '../token-helper/token-helper.service'
import { CookieService } from 'ngx-cookie'
import { AppConfig } from 'src/app/models/environment.types'
import { CurrentUserService } from '../current-user/current-user.service'

@Injectable({
  providedIn: 'root',
})
export class LanguageSelectorService {
  private languageCookieName = `${this.appConfig.appname
    .replace(/ /g, '_')
    .toLowerCase()}_selectedLang`

  constructor(
    private currentUserService: CurrentUserService,
    private cookieService: CookieService,
    private translate: TranslateService,
    private tokenHelper: TokenHelperService,
    private appConfig: AppConfig
  ) {}

  public async SetLanguage(language: string): Promise<void> {
    if (!this.tokenHelper.isTokenAnonymous()) {
      const user = this.currentUserService.get()
      if (user?.xp?.Language == language) {
        return
      }

      const patchLangXp = {
        xp: {
          Language: language,
        },
      }
      await this.currentUserService.patch(patchLangXp)
    } else {
      const selectedLang = this.cookieService
        .getObject(this.languageCookieName)
        ?.toString()
      if (selectedLang == language) {
        return
      }
    }

    this.cookieService.put(this.languageCookieName, language)
    this.SetTranslateLanguage()
  }

  /**
   * Implicitly sets the language used by the translate service
   */
  public SetTranslateLanguage(): void {
    const browserCultureLang = this.translate.getBrowserCultureLang()
    const browserLang = this.translate.getBrowserLang()
    const languages = this.translate.getLangs()
    const selectedLang = this.cookieService.get(this.languageCookieName)
    const isAnonymousUser = this.tokenHelper.isTokenAnonymous()
    let xpLang

    if (!isAnonymousUser) {
      const user = this.currentUserService.get()
      xpLang = user?.xp?.Language
    }
    if (xpLang) {
      this.useLanguage(xpLang)
    } else if (selectedLang && languages.includes(selectedLang)) {
      this.useLanguage(selectedLang)
    } else if (languages.includes(browserCultureLang)) {
      this.useLanguage(browserCultureLang)
    } else if (languages.includes(browserLang)) {
      this.useLanguage(browserLang)
    } else if (languages.includes(this.appConfig.defaultLanguage)) {
      this.useLanguage(this.appConfig.defaultLanguage)
    } else if (languages.length > 0) {
      this.useLanguage(languages[0])
    } else {
      throw new Error('Cannot identify a language to use.')
    }
  }

  private useLanguage(language: string) {
    this.translate.use(language)
    this.cookieService.put(this.languageCookieName, language)
  }
}
