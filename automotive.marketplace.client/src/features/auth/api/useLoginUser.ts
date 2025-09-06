import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { LoginUserCommand } from "../types/LoginUserCommand";
import { LoginUserResponse } from "../types/LoginUserResponse";

const loginUser = (body: LoginUserCommand) =>
  axiosClient.post<LoginUserResponse>(ENDPOINTS.AUTH.LOGIN, body);

export const useLoginUser = () =>
  useMutation({
    mutationFn: (body: LoginUserCommand) => loginUser(body),
    meta: {
      successMessage: "Successfully logged in!",
      errorMessage: "Sorry, couldn't log you in. Please try again.",
    },
  });
