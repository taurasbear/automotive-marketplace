import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { GetDrivetrainTypesResponse } from "@/types/enum/GetDrivetrainTypesResponse";
import { queryOptions } from "@tanstack/react-query";
import { enumKeys } from "../queryKeys/enumKeys";

const getDrivetrainTypes = () =>
  axiosClient.get<GetDrivetrainTypesResponse[]>(
    ENDPOINTS.ENUM.GET_DRIVETRAIN_TYPES,
  );

export const getDrivetrainTypesOptions = queryOptions({
  queryKey: enumKeys.drivetrainTypes(),
  queryFn: () => getDrivetrainTypes(),
});
