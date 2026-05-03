import React from "react";

/** Standard i18n mock — returns translation keys as-is */
export const i18nMock = {
  useTranslation: (_ns?: string) => ({
    t: (key: string) => key,
    i18n: { language: "lt", changeLanguage: vi.fn() },
  }),
  Trans: ({ i18nKey, children }: { i18nKey?: string; children?: React.ReactNode }) => (
    React.createElement("span", null, i18nKey ?? children)
  ),
  initReactI18next: { type: "3rdParty", init: vi.fn() },
};

/** Creates a mock navigate function and router mock */
export function createRouterMock() {
  const mockNavigate = vi.fn();
  return {
    mockNavigate,
    routerMock: {
      useNavigate: () => mockNavigate,
      useParams: () => ({}),
      useSearch: () => ({}),
      Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
        React.createElement("a", { href: String(to ?? "") }, children)
      ),
    },
  };
}

/** Creates mock Redux hooks with configurable auth state */
export function createReduxMock(overrides?: {
  userId?: string | null;
  accessToken?: string | null;
  permissions?: string[];
}) {
  const userId = overrides?.userId ?? null;
  const accessToken = overrides?.accessToken ?? null;
  const permissions = overrides?.permissions ?? [];
  const mockDispatch = vi.fn();

  const mockUseAppSelector = vi.fn().mockImplementation((selector: Function) => {
    const state = {
      auth: { userId, accessToken, permissions },
    };
    return selector(state);
  });

  return { mockDispatch, mockUseAppSelector };
}
