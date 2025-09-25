import { carKeys } from "@/api/queryKeys/carKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { UpdateCarCommand } from "../types/UpdateCarCommand";

const updateCar = (body: UpdateCarCommand) =>
  axiosClient.put<void>(ENDPOINTS.CAR.UPDATE, body);

export const useUpdateCar = () =>
  useMutation({
    mutationFn: updateCar,
    meta: {
      successMessage: "Successfully updated Car!",
      errorMessage: "Sorry, we couldn't update Car",
      invalidatesQuery: carKeys.all(),
    },
  });
