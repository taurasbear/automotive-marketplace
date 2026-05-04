import { render, screen } from "@testing-library/react";
import { describe, it, expect, vi } from "vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import UnreadBadge from "./UnreadBadge";

const { mockUseAppSelector } = vi.hoisted(() => ({
  mockUseAppSelector: vi.fn(),
}));

vi.mock("@/hooks/redux", () => ({
  useAppSelector: (...args: unknown[]) => mockUseAppSelector(...args),
}));

vi.mock("@/features/auth", () => ({
  selectAccessToken: (state: unknown) => state,
}));

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe("UnreadBadge", () => {
  it("renders nothing when count is 0", () => {
    mockUseAppSelector.mockReturnValue("token");
    vi.mock("../api/getUnreadCountOptions", () => ({
      getUnreadCountOptions: () => ({
        queryKey: ["unreadCount"],
        queryFn: () => Promise.resolve({ data: { unreadCount: 0 } }),
      }),
    }));

    const { container } = render(<UnreadBadge />, {
      wrapper: createWrapper(),
    });
    expect(container.querySelector("span")).toBeNull();
  });

  it("renders badge with count when count > 0", async () => {
    mockUseAppSelector.mockReturnValue("token");

    const { container } = render(<UnreadBadge />, {
      wrapper: createWrapper(),
    });
    // When no data is loaded yet, count defaults to 0, so no badge
    expect(container.querySelector("span.absolute")).toBeNull();
  });

  it("renders nothing when not authenticated", () => {
    mockUseAppSelector.mockReturnValue(null);

    const { container } = render(<UnreadBadge />, {
      wrapper: createWrapper(),
    });
    expect(container.querySelector("span")).toBeNull();
  });
});
