import { render, screen, fireEvent } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
    i18n: { language: "en" },
  }),
}));

vi.mock("@/lib/i18n/getTranslatedName", () => ({
  getTranslatedName: (translations: { language: string; name: string }[], lang: string) => {
    const t = translations.find((tr) => tr.language === lang);
    return t?.name ?? translations[0]?.name ?? "";
  },
}));

vi.mock("@/features/variantList/api/getVariantsByModelIdOptions", () => ({
  getVariantsByModelIdOptions: () => ({
    queryKey: ["variants", "model-1"],
    queryFn: () =>
      Promise.resolve({
        data: [
          {
            id: "v-1",
            fuelId: "fuel-1",
            fuelName: "Petrol",
            transmissionId: "trans-1",
            transmissionName: "Manual",
            bodyTypeId: "body-1",
            bodyTypeName: "Sedan",
            powerKw: 90,
            engineSizeMl: 1600,
            doorCount: 4,
          },
          {
            id: "v-2",
            fuelId: "fuel-2",
            fuelName: "Diesel",
            transmissionId: "trans-2",
            transmissionName: "Automatic",
            bodyTypeId: "body-2",
            bodyTypeName: "SUV",
            powerKw: 120,
            engineSizeMl: 2000,
            doorCount: 5,
          },
        ],
      }),
  }),
}));

vi.mock("@/api/enum/getAllFuelsOptions", () => ({
  getAllFuelsOptions: {
    queryKey: ["fuels"],
    queryFn: () => Promise.resolve({ data: [] }),
  },
}));

vi.mock("@/api/enum/getAllTransmissionsOptions", () => ({
  getAllTransmissionsOptions: {
    queryKey: ["transmissions"],
    queryFn: () => Promise.resolve({ data: [] }),
  },
}));

vi.mock("@/api/enum/getAllBodyTypesOptions", () => ({
  getAllBodyTypesOptions: {
    queryKey: ["bodyTypes"],
    queryFn: () => Promise.resolve({ data: [] }),
  },
}));

vi.mock("@/components/ui/table", () => ({
  Table: ({ children }: { children: React.ReactNode }) => (
    <table>{children}</table>
  ),
  TableBody: ({ children }: { children: React.ReactNode }) => (
    <tbody>{children}</tbody>
  ),
  TableCell: ({
    children,
    colSpan,
  }: {
    children: React.ReactNode;
    colSpan?: number;
    className?: string;
  }) => <td colSpan={colSpan}>{children}</td>,
  TableHead: ({ children }: { children: React.ReactNode }) => (
    <th>{children}</th>
  ),
  TableHeader: ({ children }: { children: React.ReactNode }) => (
    <thead>{children}</thead>
  ),
  TableRow: ({
    children,
    onClick,
    className,
  }: {
    children: React.ReactNode;
    onClick?: () => void;
    className?: string;
  }) => (
    <tr onClick={onClick} className={className}>
      {children}
    </tr>
  ),
}));

import VariantTable from "./VariantTable";

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("VariantTable", () => {
  const defaultProps = {
    modelId: "model-1",
    selectedVariantId: "",
    onSelect: vi.fn(),
  };

  it("renders nothing when modelId is empty", () => {
    const { container } = render(
      <VariantTable {...defaultProps} modelId="" />,
      { wrapper: createWrapper() },
    );
    expect(container.innerHTML).toBe("");
  });

  it("renders nothing when disabled", () => {
    const { container } = render(
      <VariantTable {...defaultProps} disabled />,
      { wrapper: createWrapper() },
    );
    expect(container.innerHTML).toBe("");
  });

  it("renders table headers", () => {
    render(<VariantTable {...defaultProps} />, { wrapper: createWrapper() });
    expect(screen.getByText("variantTable.fuel")).toBeInTheDocument();
    expect(screen.getByText("variantTable.transmission")).toBeInTheDocument();
    expect(screen.getByText("variantTable.powerKw")).toBeInTheDocument();
    expect(screen.getByText("variantTable.engineMl")).toBeInTheDocument();
    expect(screen.getByText("variantTable.bodyType")).toBeInTheDocument();
    expect(screen.getByText("variantTable.doors")).toBeInTheDocument();
  });

  it("renders variant rows with data", async () => {
    render(<VariantTable {...defaultProps} />, { wrapper: createWrapper() });
    expect(await screen.findByText("90")).toBeInTheDocument();
    expect(screen.getByText("1600")).toBeInTheDocument();
    expect(screen.getByText("120")).toBeInTheDocument();
    expect(screen.getByText("2000")).toBeInTheDocument();
  });

  it("calls onSelect with variant on row click", async () => {
    const onSelect = vi.fn();
    render(
      <VariantTable {...defaultProps} onSelect={onSelect} />,
      { wrapper: createWrapper() },
    );
    const row = await screen.findByText("90");
    fireEvent.click(row.closest("tr")!);
    expect(onSelect).toHaveBeenCalledWith(
      expect.objectContaining({ id: "v-1" }),
    );
  });

  it("calls onSelect with null when clicking already selected variant", async () => {
    const onSelect = vi.fn();
    render(
      <VariantTable {...defaultProps} onSelect={onSelect} selectedVariantId="v-1" />,
      { wrapper: createWrapper() },
    );
    const row = await screen.findByText("90");
    fireEvent.click(row.closest("tr")!);
    expect(onSelect).toHaveBeenCalledWith(null);
  });
});
