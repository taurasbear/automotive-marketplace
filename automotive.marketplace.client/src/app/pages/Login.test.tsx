import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { z } from "zod";
import Login from "./Login";

const { mockLoginAsync, mockDispatch, mockNavigate } = vi.hoisted(() => ({
  mockLoginAsync: vi.fn(),
  mockDispatch: vi.fn(),
  mockNavigate: vi.fn(),
}));

vi.mock("@/features/auth", () => ({
  useLoginUser: () => ({ mutateAsync: mockLoginAsync }),
  LoginSchema: z.object({
    email: z.string().email(),
    password: z.string().min(6),
  }),
  setCredentials: (payload: Record<string, unknown>) => ({
    type: "auth/setCredentials",
    payload,
  }),
}));

vi.mock("@/hooks/redux", () => ({
  useAppDispatch: () => mockDispatch,
  useAppSelector: vi.fn(),
}));

vi.mock("@tanstack/react-router", () => ({
  useNavigate: () => mockNavigate,
  Link: ({ children, to }: { children: React.ReactNode; to?: string }) => (
    <a href={String(to ?? "")}>{children}</a>
  ),
}));

vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (key: string) => key,
  }),
}));

beforeEach(() => {
  mockLoginAsync.mockReset();
  mockDispatch.mockReset();
  mockNavigate.mockReset();
});

describe("Login page", () => {
  it("renders email and password fields and a submit button", () => {
    render(<Login />);
    expect(screen.getByPlaceholderText("login.fields.emailPlaceholder")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("login.fields.passwordPlaceholder")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "login.submit" })).toBeInTheDocument();
  });

  it("toggles password visibility when the eye icon is clicked", async () => {
    const user = userEvent.setup();
    render(<Login />);

    const passwordInput = screen.getByPlaceholderText("login.fields.passwordPlaceholder");
    expect(passwordInput).toHaveAttribute("type", "password");

    await user.click(screen.getByLabelText("login.fields.showPassword"));
    expect(passwordInput).toHaveAttribute("type", "text");

    await user.click(screen.getByLabelText("login.fields.hidePassword"));
    expect(passwordInput).toHaveAttribute("type", "password");
  });

  it("calls loginUserAsync, dispatches setCredentials, and navigates on successful submit", async () => {
    mockLoginAsync.mockResolvedValue({
      data: { accessToken: "tok-123", permissions: ["admin"], userId: "user-1" },
    });

    const user = userEvent.setup();
    render(<Login />);

    await user.type(screen.getByPlaceholderText("login.fields.emailPlaceholder"), "test@example.com");
    await user.type(screen.getByPlaceholderText("login.fields.passwordPlaceholder"), "Password123!");
    await user.click(screen.getByRole("button", { name: "login.submit" }));

    await waitFor(() => {
      expect(mockLoginAsync).toHaveBeenCalledWith({
        email: "test@example.com",
        password: "Password123!",
      });
    });

    expect(mockDispatch).toHaveBeenCalledWith(
      expect.objectContaining({
        payload: { accessToken: "tok-123", permissions: ["admin"], userId: "user-1" },
      }),
    );
    expect(mockNavigate).toHaveBeenCalledWith({ to: "/" });
  });
});
