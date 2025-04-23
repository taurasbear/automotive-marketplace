import { useMutation } from "@tanstack/react-query";
import AuthService from "../services/AuthService";
import { RegisterAccountRequest } from "@/shared/types/dto/Auth/RegisterAccountRequest";

export const useRegister = () => {
  return useMutation({
    mutationFn: ({ request }: { request: RegisterAccountRequest }) =>
      AuthService.Register(request),
  });
};

export const useRefresh = () => {
  return useMutation({
    mutationFn: () => AuthService.Refresh(),
  });
};
