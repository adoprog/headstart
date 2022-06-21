import { Injectable } from '@angular/core'
import { TranslateService } from '@ngx-translate/core'
import { TokenHelperService } from '../token-helper/token-helper.service'
import { CookieService } from 'ngx-cookie'
import { MeUser, Me, Tokens } from 'ordercloud-javascript-sdk'
import { AppConfig } from 'src/app/models/environment.types'

@Injectable({
  providedIn: 'root',
})
export class LanguageSelectorService {
  private languageCookieName = `${this.appConfig.appname
    .replace(/ /g, '_')
    .toLowerCase()}_selectedLang`

  /**
   * @ignore
   * not part of public api, don't include in generated docs
   */
  constructor(
    private cookieService: CookieService,
    private translate: TranslateService,
    private tokenHelper: TokenHelperService,
    private appConfig: AppConfig
  ) {
    this.SetLanguage = this.SetLanguage.bind(this)
    this.SetTranslateLanguage = this.SetTranslateLanguage.bind(this)
  }

  public async SetLanguage(language: string, user?: MeUser): Promise<void> {
    if (!this.tokenHelper.isTokenAnonymous()) {
      if (user?.xp?.Language == language) {
        return
      }

      const accessToken = Tokens.GetAccessToken()
      const patchLangXp = {
        xp: {
          Language: language
        }
      }
      await Me.Patch(patchLangXp, { accessToken: accessToken })
    } else {
      const selectedLang = this.cookieService.getObject(this.languageCookieName)?.toString()
      if (selectedLang == language) {
        return
      }
    }

    this.cookieService.putObject(this.languageCookieName, language)
    this.SetTranslateLanguage()
  }

  /**
   * Implicitly sets the language used by the translate service
   */
  public async SetTranslateLanguage(): Promise<void> {
    const browserCultureLang = this.translate.getBrowserCultureLang();
    const browserLang = this.translate.getBrowserLang();
    const languages = this.translate.getLangs()
    const selectedLang = this.cookieService.getObject(this.languageCookieName)?.toString()
    const accessToken = Tokens.GetAccessToken()
    const isAnonymousUser = this.tokenHelper.isTokenAnonymous()
    let xpLang
      
    if (!isAnonymousUser && accessToken){
      const user = await Me.Get({ accessToken: accessToken })
      xpLang = user?.xp?.Language
    }
    if (xpLang) {
      this.translate.use(xpLang);
    } else if (selectedLang && languages.includes(selectedLang)) {
      this.translate.use(selectedLang);
    } else if (languages.includes(browserCultureLang)) {
      this.translate.use(browserCultureLang);
    } else if (languages.includes(browserLang)) {
      this.translate.use(browserLang);
    } else if (languages.includes(this.appConfig.defaultLanguage)) {
      this.translate.use(this.appConfig.defaultLanguage)
    } else if (languages.length > 0) {
      this.translate.use(languages[0])
    } else {
      throw new Error('Cannot identify a language to use.')
    }
  }
}
