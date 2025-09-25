import { carKeys } from "@/api/queryKeys/carKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { CreateCarCommand } from "../types/CreateCarCommand";

const createCar = (body: CreateCarCommand) =>
  axiosClient.post<void>(ENDPOINTS.CAR.CREATE, body);

export const useCreateCar = () =>
  useMutation({
    mutationFn: createCar,
    meta: {
      successMessage: "Successfully created Car!",
      errorMessage: "Sorry, we couldn't create your Car",
      invalidatesQuery: carKeys.all(),
    },
  });
