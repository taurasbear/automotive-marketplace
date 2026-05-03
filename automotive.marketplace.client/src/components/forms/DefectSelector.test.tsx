import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

const { mockAddDefect, mockRemoveDefect, mockAddImage, mockRemoveImage } =
  vi.hoisted(() => ({
    mockAddDefect: vi.fn().mockResolvedValue(undefined),
    mockRemoveDefect: vi.fn().mockResolvedValue(undefined),
    mockAddImage: vi.fn().mockResolvedValue(undefined),
    mockRemoveImage: vi.fn().mockResolvedValue(undefined),
  }));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
    i18n: { language: "en" },
  }),
}));

vi.mock("@/api/defect/getDefectCategoriesOptions", () => ({
  getDefectCategoriesOptions: {
    queryKey: ["defectCategories"],
    queryFn: () =>
      Promise.resolve({
        data: [
          { id: "cat-1", translations: [{ language: "en", name: "Scratch" }] },
          { id: "cat-2", translations: [{ language: "en", name: "Dent" }] },
        ],
      }),
  },
}));

vi.mock("@/api/defect/useAddListingDefect", () => ({
  useAddListingDefect: () => ({ mutateAsync: mockAddDefect }),
}));

vi.mock("@/api/defect/useRemoveListingDefect", () => ({
  useRemoveListingDefect: () => ({ mutateAsync: mockRemoveDefect }),
}));

vi.mock("@/api/defect/useAddDefectImage", () => ({
  useAddDefectImage: () => ({ mutateAsync: mockAddImage }),
}));

vi.mock("@/api/defect/useRemoveDefectImage", () => ({
  useRemoveDefectImage: () => ({ mutateAsync: mockRemoveImage }),
}));

vi.mock("@/lib/i18n/getTranslatedName", () => ({
  getTranslatedName: (translations: { language: string; name: string }[], lang: string) => {
    const t = translations.find((tr) => tr.language === lang);
    return t?.name ?? translations[0]?.name ?? "";
  },
}));

vi.mock("@/components/ui/button", () => ({
  Button: ({
    children,
    onClick,
    disabled,
    ...props
  }: {
    children: React.ReactNode;
    onClick?: () => void;
    disabled?: boolean;
    [key: string]: unknown;
  }) => (
    <button onClick={onClick} disabled={disabled} type={props.type as string}>
      {children}
    </button>
  ),
}));

vi.mock("@/components/ui/checkbox", () => ({
  Checkbox: ({
    id,
    checked,
    onCheckedChange,
  }: {
    id: string;
    checked: boolean;
    onCheckedChange: () => void;
  }) => (
    <input
      type="checkbox"
      id={id}
      checked={checked}
      onChange={onCheckedChange}
      data-testid={`checkbox-${id}`}
    />
  ),
}));

vi.mock("@/components/ui/input", () => ({
  Input: (props: React.InputHTMLAttributes<HTMLInputElement>) => (
    <input {...props} data-testid="custom-defect-input" />
  ),
}));

import DefectSelector from "./DefectSelector";

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("DefectSelector", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe("form mode", () => {
    it("renders title and category checkboxes", async () => {
      const onDefectsChange = vi.fn();
      render(
        <DefectSelector
          mode="form"
          selectedDefects={[]}
          onDefectsChange={onDefectsChange}
        />,
        { wrapper: createWrapper() },
      );
      expect(screen.getByText("defectSelector.title")).toBeInTheDocument();
      expect(await screen.findByText("Scratch")).toBeInTheDocument();
      expect(await screen.findByText("Dent")).toBeInTheDocument();
    });

    it("renders custom defect input", () => {
      const onDefectsChange = vi.fn();
      render(
        <DefectSelector
          mode="form"
          selectedDefects={[]}
          onDefectsChange={onDefectsChange}
        />,
        { wrapper: createWrapper() },
      );
      expect(screen.getByTestId("custom-defect-input")).toBeInTheDocument();
      expect(screen.getByText("defectSelector.addCustom")).toBeInTheDocument();
    });

    it("toggles defect category on checkbox change", async () => {
      const onDefectsChange = vi.fn();
      render(
        <DefectSelector
          mode="form"
          selectedDefects={[]}
          onDefectsChange={onDefectsChange}
        />,
        { wrapper: createWrapper() },
      );
      const checkbox = await screen.findByTestId("checkbox-cat-1");
      fireEvent.click(checkbox);
      expect(onDefectsChange).toHaveBeenCalledWith([
        { defectCategoryId: "cat-1", images: [] },
      ]);
    });

    it("adds custom defect on button click", () => {
      const onDefectsChange = vi.fn();
      render(
        <DefectSelector
          mode="form"
          selectedDefects={[]}
          onDefectsChange={onDefectsChange}
        />,
        { wrapper: createWrapper() },
      );
      const input = screen.getByTestId("custom-defect-input");
      fireEvent.change(input, { target: { value: "Rust spot" } });
      fireEvent.click(screen.getByText("defectSelector.addCustom"));
      expect(onDefectsChange).toHaveBeenCalledWith([
        { customName: "Rust spot", images: [] },
      ]);
    });

    it("shows no defects message when empty", () => {
      const onDefectsChange = vi.fn();
      render(
        <DefectSelector
          mode="form"
          selectedDefects={[]}
          onDefectsChange={onDefectsChange}
        />,
        { wrapper: createWrapper() },
      );
      expect(screen.getByText("defectSelector.noDefects")).toBeInTheDocument();
    });
  });
});
