import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import ViewVariantDialogContent from "./ViewVariantDialogContent";
import type { Variant } from "../types/Variant";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
}));

vi.mock("@/components/ui/dialog", () => ({
  DialogHeader: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="dialog-header">{children}</div>
  ),
  DialogTitle: ({ children }: { children: React.ReactNode }) => (
    <h2 data-testid="dialog-title">{children}</h2>
  ),
  DialogDescription: ({ children }: { children: React.ReactNode }) => (
    <p data-testid="dialog-description">{children}</p>
  ),
}));

const mockVariant: Variant = {
  id: "var-1",
  modelId: "model-1",
  fuelId: "fuel-1",
  fuelName: "Diesel",
  transmissionId: "trans-1",
  transmissionName: "Manual",
  bodyTypeId: "body-1",
  bodyTypeName: "Sedan",
  doorCount: 4,
  powerKw: 110,
  engineSizeMl: 2000,
  isCustom: false,
};

describe("ViewVariantDialogContent", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders dialog title and description", () => {
    render(<ViewVariantDialogContent variant={mockVariant} />);

    expect(screen.getByText("variants.variantDetails")).toBeInTheDocument();
    expect(screen.getByText("variants.readOnly")).toBeInTheDocument();
  });

  it("renders fuel name", () => {
    render(<ViewVariantDialogContent variant={mockVariant} />);

    expect(screen.getByText(/Diesel/)).toBeInTheDocument();
  });

  it("renders transmission name", () => {
    render(<ViewVariantDialogContent variant={mockVariant} />);

    expect(screen.getByText(/Manual/)).toBeInTheDocument();
  });

  it("renders body type name", () => {
    render(<ViewVariantDialogContent variant={mockVariant} />);

    expect(screen.getByText(/Sedan/)).toBeInTheDocument();
  });

  it("renders numeric specs", () => {
    render(<ViewVariantDialogContent variant={mockVariant} />);

    expect(screen.getByText(/4/)).toBeInTheDocument();
    expect(screen.getByText(/110/)).toBeInTheDocument();
    expect(screen.getByText(/2000/)).toBeInTheDocument();
  });

  it("renders custom flag as no when isCustom is false", () => {
    render(<ViewVariantDialogContent variant={mockVariant} />);

    expect(screen.getByText(/variants.no/)).toBeInTheDocument();
  });

  it("renders custom flag as yes when isCustom is true", () => {
    render(
      <ViewVariantDialogContent variant={{ ...mockVariant, isCustom: true }} />
    );

    expect(screen.getByText(/variants.yes/)).toBeInTheDocument();
  });
});
