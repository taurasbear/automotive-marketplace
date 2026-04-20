// TODO: Used by the Create/Edit Variant form (not yet implemented)
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { Drivetrain } from "@/types/enum/Drivetrain";
import { queryOptions } from "@tanstack/react-query";
import { enumKeys } from "../queryKeys/enumKeys";

const getAllDrivetrains = () =>
  axiosClient.get<Drivetrain[]>(ENDPOINTS.DRIVETRAIN.GET_ALL);

export const getAllDrivetrainsOptions = queryOptions({
  queryKey: enumKeys.drivetrains(),
  queryFn: getAllDrivetrains,
});
