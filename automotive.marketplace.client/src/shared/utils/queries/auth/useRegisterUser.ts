import { RegisterUserCommand } from "@/shared/types/dto/auth/RegisterUserCommand";
import { useMutation } from "@tanstack/react-query";
import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { RegisterUserResponse } from "@/shared/types/dto/auth/RegisterUserResponse";

const registerUser = (body: RegisterUserCommand) =>
  axiosClient.post<RegisterUserResponse>(ENDPOINTS.AUTH.REGISTER, body);

export const useRegisterUser = () =>
  useMutation({
    mutationFn: registerUser,
    meta: {
      successMessage: "Successfully registered!",
    },
  });
