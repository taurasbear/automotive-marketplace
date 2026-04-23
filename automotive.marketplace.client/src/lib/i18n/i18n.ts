import i18n from "i18next";
import LanguageDetector from "i18next-browser-languagedetector";
import { initReactI18next } from "react-i18next";

import adminEn from "./locales/en/admin.json";
import authEn from "./locales/en/auth.json";
import chatEn from "./locales/en/chat.json";
import commonEn from "./locales/en/common.json";
import compareEn from "./locales/en/compare.json";
import listingsEn from "./locales/en/listings.json";
import savedEn from "./locales/en/saved.json";
import validationEn from "./locales/en/validation.json";

import adminLt from "./locales/lt/admin.json";
import authLt from "./locales/lt/auth.json";
import chatLt from "./locales/lt/chat.json";
import commonLt from "./locales/lt/common.json";
import compareLt from "./locales/lt/compare.json";
import listingsLt from "./locales/lt/listings.json";
import savedLt from "./locales/lt/saved.json";
import validationLt from "./locales/lt/validation.json";

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      en: {
        common: commonEn,
        auth: authEn,
        chat: chatEn,
        listings: listingsEn,
        saved: savedEn,
        compare: compareEn,
        admin: adminEn,
        validation: validationEn,
      },
      lt: {
        common: commonLt,
        auth: authLt,
        chat: chatLt,
        listings: listingsLt,
        saved: savedLt,
        compare: compareLt,
        admin: adminLt,
        validation: validationLt,
      },
    },
    fallbackLng: "en",
    defaultNS: "common",
    detection: {
      order: ["localStorage"],
      lookupLocalStorage: "language",
      caches: ["localStorage"],
    },
    interpolation: {
      escapeValue: false,
    },
  });

export default i18n;
