import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, vi, beforeEach } from "vitest";
import VariantForm from "./VariantForm";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({ t: (key: string) => key }),
  initReactI18next: { type: "3rdParty", init: () => {} },
}));

vi.mock("@/components/forms/select/MakeSelect", () => ({
  default: ({
    onValueChange,
    value,
  }: {
    onValueChange: (v: string) => void;
    value?: string;
  }) => (
    <select
      data-testid="make-select"
      value={value}
      onChange={(e) => onValueChange(e.target.value)}
    >
      <option value="">Select</option>
      <option value="make-1">Toyota</option>
    </select>
  ),
}));

vi.mock("@/components/forms/select/ModelSelect", () => ({
  default: ({
    onValueChange,
    value,
    disabled,
  }: {
    onValueChange: (v: string) => void;
    value?: string;
    disabled?: boolean;
  }) => (
    <select
      data-testid="model-select"
      value={value}
      disabled={disabled}
      onChange={(e) => onValueChange(e.target.value)}
    >
      <option value="">Select</option>
      <option value="model-1">Corolla</option>
    </select>
  ),
}));

vi.mock("@/components/forms/select/FuelSelect", () => ({
  default: ({
    onValueChange,
    value,
  }: {
    onValueChange: (v: string) => void;
    value?: string;
  }) => (
    <select
      data-testid="fuel-select"
      value={value}
      onChange={(e) => onValueChange(e.target.value)}
    >
      <option value="">Select</option>
      <option value="fuel-1">Diesel</option>
    </select>
  ),
}));

vi.mock("@/components/forms/TransmissionToggleGroup", () => ({
  default: ({
    onValueChange,
    value,
  }: {
    onValueChange: (v: string) => void;
    value?: string;
  }) => (
    <select
      data-testid="transmission-toggle"
      value={value}
      onChange={(e) => onValueChange(e.target.value)}
    >
      <option value="">Select</option>
      <option value="trans-1">Manual</option>
    </select>
  ),
}));

vi.mock("@/components/forms/select/BodyTypeSelect", () => ({
  default: ({
    onValueChange,
    value,
  }: {
    onValueChange: (v: string) => void;
    value?: string;
  }) => (
    <select
      data-testid="bodytype-select"
      value={value}
      onChange={(e) => onValueChange(e.target.value)}
    >
      <option value="">Select</option>
      <option value="body-1">Sedan</option>
    </select>
  ),
}));

const defaultVariant = {
  makeId: "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  modelId: "b2c3d4e5-f6a7-8901-bcde-f12345678901",
  fuelId: "c3d4e5f6-a7b8-9012-cdef-123456789012",
  transmissionId: "d4e5f6a7-b8c9-0123-defa-234567890123",
  bodyTypeId: "e5f6a7b8-c9d0-1234-efab-345678901234",
  doorCount: 4,
  powerKw: 100,
  engineSizeMl: 1600,
  isCustom: false,
};

describe("VariantForm", () => {
  const mockOnSubmit = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    mockOnSubmit.mockResolvedValue(undefined);
  });

  it("renders all form fields", () => {
    render(<VariantForm variant={defaultVariant} onSubmit={mockOnSubmit} />);

    expect(screen.getByTestId("make-select")).toBeInTheDocument();
    expect(screen.getByTestId("model-select")).toBeInTheDocument();
    expect(screen.getByTestId("fuel-select")).toBeInTheDocument();
    expect(screen.getByTestId("transmission-toggle")).toBeInTheDocument();
    expect(screen.getByTestId("bodytype-select")).toBeInTheDocument();
    expect(screen.getByLabelText("admin:variants.doors")).toBeInTheDocument();
    expect(screen.getByLabelText("admin:variants.powerKw")).toBeInTheDocument();
    expect(
      screen.getByLabelText("admin:variants.engineSizeMl")
    ).toBeInTheDocument();
  });

  it("renders submit button", () => {
    render(<VariantForm variant={defaultVariant} onSubmit={mockOnSubmit} />);

    expect(
      screen.getByRole("button", { name: "common:actions.confirm" })
    ).toBeInTheDocument();
  });

  it("renders with pre-filled numeric values", () => {
    render(<VariantForm variant={defaultVariant} onSubmit={mockOnSubmit} />);

    expect(screen.getByLabelText("admin:variants.doors")).toHaveValue(4);
    expect(screen.getByLabelText("admin:variants.powerKw")).toHaveValue(100);
    expect(screen.getByLabelText("admin:variants.engineSizeMl")).toHaveValue(
      1600
    );
  });

  it("calls onSubmit with form data on valid submit", async () => {
    render(<VariantForm variant={defaultVariant} onSubmit={mockOnSubmit} />);

    fireEvent.click(
      screen.getByRole("button", { name: "common:actions.confirm" })
    );

    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith(defaultVariant);
    });
  });
});
